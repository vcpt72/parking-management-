using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Parking_projekt.Models;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Parking_projekt.Controllers
{
    
    [ApiController]
    [Route("api/")]
    public class ApiController : Controller
    {
        private readonly string _connectionString = "Data Source=parkingdb.db;";


        [ProducesResponseType(typeof(ParkingLot), 200)]
        [HttpGet("parkinglots")]
        public async Task<IActionResult> GetParkingLots()
        {
            using var connection = new SqliteConnection(_connectionString);
            var lots = await connection.QueryAsync<ParkingLot>("SELECT * FROM ParkingLots");
            return Ok(lots);
        }

        [ProducesResponseType(typeof(ParkingSpot), 200)]
        [HttpGet("parkingspots")]
        public async Task<IActionResult> GetParkingSpots()
        {
            using var connection = new SqliteConnection(_connectionString);
            var lots = await connection.QueryAsync<ParkingSpot>("SELECT * FROM ParkingSpots");
            return Ok(lots);
        }


        [ProducesResponseType(typeof(AppUser), 200)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AppUser model)
        {
            using var connection = new SqliteConnection(_connectionString);
            var user = await connection.QueryFirstOrDefaultAsync<AppUser>(
                "SELECT * FROM Users WHERE Username = @Username AND Password = @Password",
                new { model.Username, model.Password });

            if (user == null)
            {
                return Unauthorized("Neplatné přihlašovací údaje.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(user);
        }

        [ProducesResponseType(typeof(string), 200)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AppUser newUser)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            if (string.IsNullOrWhiteSpace(newUser.Username) || string.IsNullOrWhiteSpace(newUser.Password))
                return BadRequest(new { message = "Uživatelské jméno a heslo jsou povinné." });

            var existingUser = await connection.QueryFirstOrDefaultAsync<AppUser>(
                "SELECT * FROM Users WHERE Username = @Username",
                new { newUser.Username });

            if (existingUser != null)
                return Conflict(new { message = "Uživatel s tímto jménem již existuje." });

            
            newUser.Role = UserRole.ReadOnly;

            await connection.ExecuteAsync(
                "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role)",
                newUser);

            return Ok( "Registrace proběhla úspěšně." );
        }

        [ProducesResponseType(typeof(string), 200)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok("Uživatel byl úspěšně odhlášen.");
        }

        [ProducesResponseType(typeof(ParkingLot), 200)]
        [HttpGet("parkinglots/{id}")]
        public async Task<IActionResult> GetParkingLotById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);

            var parkingLot = await connection.QueryFirstOrDefaultAsync<ParkingLot>(
                "SELECT * FROM ParkingLots WHERE Id = @Id", new { Id = id });

            if (parkingLot == null)
                return NotFound();

            var spots = (await connection.QueryAsync<ParkingSpot>(
                "SELECT * FROM ParkingSpots WHERE ParkingLotId = @Id", new { Id = id })).ToList();

            
            for (int i = 0; i < spots.Count; i++)
            {
                spots[i].LocalId = i + 1; 
            }

            parkingLot.ParkingSpots = spots;

            return Ok(parkingLot);
        }

        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ParkingSpot), 200)]
        [HttpPost("parkinglots/{parkingLotId}/addspot")]
        public async Task<IActionResult> AddParkingSpot(int parkingLotId)
        {
            using var connection = new SqliteConnection(_connectionString);

            
            var newSpot = new ParkingSpot
            {
                ParkingLotId = parkingLotId,
                Status = ParkingSpotStatus.Available
            };

            
            var result = await connection.ExecuteAsync(
                "INSERT INTO ParkingSpots (ParkingLotId, Status) VALUES (@ParkingLotId, @Status)",
                new { ParkingLotId = parkingLotId, Status = newSpot.Status });

           
                return Ok(newSpot);
        }

        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(string), 200)]
        [HttpDelete("parkinglots/{parkingLotId}/removespot/{spotId}")]
        public async Task<IActionResult> RemoveParkingSpot(int parkingLotId, int spotId)
        {
            using var connection = new SqliteConnection(_connectionString);

            
            var existingSpot = await connection.QueryFirstOrDefaultAsync<ParkingSpot>(
                "SELECT * FROM ParkingSpots WHERE Id = @SpotId AND ParkingLotId = @ParkingLotId",
                new { SpotId = spotId, ParkingLotId = parkingLotId });

            if (existingSpot == null)
            {
                return NotFound("Parkovací místo nebylo nalezeno.");
            }

            
            var result = await connection.ExecuteAsync(
                "DELETE FROM ParkingSpots WHERE Id = @SpotId",
                new { SpotId = spotId });

            
                return Ok("Parkovací místo bylo úspěšně odstraněno.");
           
        }


        
        [ProducesResponseType(typeof(ParkingSpot), 200)]
        //[Authorize(Roles ="Admin")]
        [HttpPut("parkinglots/{parkingLotId}/spot/{spotId}/maintenance")]
        public async Task<IActionResult> SetParkingSpotToMaintenance(int parkingLotId, int spotId)
        {
            using var connection = new SqliteConnection(_connectionString);

            
            var spot = await connection.QueryFirstOrDefaultAsync<ParkingSpot>(
                "SELECT * FROM ParkingSpots WHERE Id = @SpotId AND ParkingLotId = @ParkingLotId",
                new { SpotId = spotId, ParkingLotId = parkingLotId });

            if (spot == null)
                return NotFound();

            if (spot.Status == ParkingSpotStatus.Occupied)
            {
                return BadRequest("Nelze změnit stav parkovacího místa, protože je již obsazené.");
            }
            var oldStatus = spot.Status;
            spot.Status = ParkingSpotStatus.Maintenance;

            
            await connection.ExecuteAsync(
                "UPDATE ParkingSpots SET Status = @Status WHERE Id = @SpotId",
                new { Status = spot.Status, SpotId = spotId });


            await connection.ExecuteAsync(
            "INSERT INTO ParkingSpotHistory (ParkingSpotId, OldStatus, NewStatus, ChangedAt) VALUES (@ParkingSpotId, @OldStatus, @NewStatus, @ChangedAt)",
            new
            {
                ParkingSpotId = spot.Id,
                OldStatus = oldStatus,
                NewStatus = spot.Status,
                ChangedAt = DateTime.UtcNow
            });

            return Ok(spot); 
        }


        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ParkingSpot), 200)]
        [HttpPut("parkinglots/{parkingLotId}/spot/{spotId}/available")]
        public async Task<IActionResult> SetParkingSpotToAvailable(int parkingLotId, int spotId)
        {
            using var connection = new SqliteConnection(_connectionString);


            var spot = await connection.QueryFirstOrDefaultAsync<ParkingSpot>(
                "SELECT * FROM ParkingSpots WHERE Id = @SpotId AND ParkingLotId = @ParkingLotId",
                new { SpotId = spotId, ParkingLotId = parkingLotId });

            if (spot == null)
                return NotFound();

            if (spot.Status == ParkingSpotStatus.Occupied)
            {
                return BadRequest("Nelze změnit stav parkovacího místa, protože je již obsazené.");
            }

            var oldStatus = spot.Status;
            spot.Status = ParkingSpotStatus.Available;


            await connection.ExecuteAsync(
                "UPDATE ParkingSpots SET Status = @Status WHERE Id = @SpotId",
                new { Status = spot.Status, SpotId = spotId });

            await connection.ExecuteAsync(
            "INSERT INTO ParkingSpotHistory (ParkingSpotId, OldStatus, NewStatus, ChangedAt) VALUES (@ParkingSpotId, @OldStatus, @NewStatus, @ChangedAt)",
            new
            {
                ParkingSpotId = spot.Id,
                OldStatus = oldStatus,
                NewStatus = spot.Status,
                ChangedAt = DateTime.UtcNow
            });
            return Ok(spot);
        }


        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [HttpGet("description")]
        public IActionResult GetApiDescription()
        {
            var apiDescription = new List<object>();

            
            var controllers = typeof(ApiController).Assembly.GetTypes()
                .Where(type => typeof(ApiController).IsAssignableFrom(type) && type.Name.EndsWith("Controller"));

            foreach (var controller in controllers)
            {
                var controllerRouteAttr = controller.GetCustomAttribute<RouteAttribute>();
                var controllerRouteTemplate = controllerRouteAttr?.Template ?? "[controller]";

                string controllerName = controller.Name.Replace("Controller", "");
                string controllerRoute = controllerRouteTemplate.Replace("[controller]", controllerName).Trim('/');

                var methods = controller.GetMethods()
                    .Where(m => m.IsPublic && !m.IsDefined(typeof(NonActionAttribute)));

                foreach (var method in methods)
                {
                    var httpMethodAttr = method.GetCustomAttributes()
                        .FirstOrDefault(attr => attr is HttpGetAttribute || attr is HttpPostAttribute || attr is HttpPutAttribute || attr is HttpDeleteAttribute);

                    if (httpMethodAttr == null) continue;

                    var httpMethodName = httpMethodAttr.GetType().Name.Replace("Attribute", "");

                    string actionRoute = null;

                    if (httpMethodAttr is HttpMethodAttribute methodAttribute && !string.IsNullOrEmpty(methodAttribute.Template))
                    {
                        actionRoute = methodAttribute.Template;
                    }
                    else
                    {
                        actionRoute = method.GetCustomAttributes<RouteAttribute>().FirstOrDefault()?.Template ?? method.Name;
                    }

                    string fullRoute = "/" + string.Join("/", new[] { controllerRoute, actionRoute }.Where(s => !string.IsNullOrEmpty(s)));

                    var parameters = method.GetParameters()
                        .Select(p => new
                        {
                            Name = p.Name,
                            Type = p.ParameterType.Name
                        }).ToList();

                    string returnTypeName;

                    var returnType = method.ReturnType;

                    if (typeof(Task).IsAssignableFrom(returnType))
                    {
                        if (returnType.IsGenericType)
                        {
                            returnType = returnType.GetGenericArguments()[0];
                        }
                        else
                        {
                            returnTypeName = "void";
                            goto AddEntry;
                        }
                    }

                    if (returnType == typeof(IActionResult) || returnType == typeof(ActionResult))
                    {
                        var producesAttr = method.GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
                                                 .Cast<ProducesResponseTypeAttribute>()
                                                 .FirstOrDefault(attr => attr.StatusCode == 200);

                        if (producesAttr?.Type != null && producesAttr.Type != typeof(void))
                        {
                            returnTypeName = producesAttr.Type.Name;
                        }
                        else
                        {
                            returnTypeName = "IActionResult";
                        }
                    }
                    else
                    {
                        returnTypeName = returnType.Name;
                    }

                AddEntry:
                    apiDescription.Add(new
                    {
                        Controller = controller.Name,
                        Action = method.Name,
                        HttpMethod = httpMethodName,
                        Route = fullRoute,
                        Parameters = parameters,
                        ReturnType = returnTypeName
                    });
                }
            }

            return Ok(apiDescription);
        }

        //[Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ParkingSpotHistory), 200)]
        [HttpGet("parkingspots/{spotId}/history")]
        public async Task<IActionResult> GetParkingSpotHistory(int spotId)
        {

            using var connection = new SqliteConnection(_connectionString);

            var history = await connection.QueryAsync<ParkingSpotHistory>(
                "SELECT * FROM ParkingSpotHistory WHERE ParkingSpotId = @SpotId ORDER BY ChangedAt DESC",
                new { SpotId = spotId });

            return Ok(history);
        }

        [ProducesResponseType(typeof(int), 200)]
        [HttpGet("parkinglots/{id}/stats")]
        public async Task<IActionResult> GetParkingStats(int id)
        {
           
            using var connection = new SqliteConnection(_connectionString);
            var monthAgo = DateTime.UtcNow.AddMonths(-1);

            var count = await connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(*) 
        FROM ParkingOccupations 
        WHERE ParkingSpotId IN (
            SELECT Id FROM ParkingSpots WHERE ParkingLotId = @Id
        )
        AND EndTime IS NOT NULL 
        AND EndTime >= datetime('now', '-1 month')",
                new { Id = id });

            

            return Ok(new { EndedOccupationsLastMonth = count });
        }
    }


   
}
