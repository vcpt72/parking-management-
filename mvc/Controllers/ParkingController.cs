    using Dapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.Sqlite;
    using Parking_projekt.Models;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Json;

    namespace Parking_projekt.Controllers
    {
        public class ParkingController : Controller
        {
            private readonly HttpClient _httpClient;
            private readonly string _baseApiUrl = "https://localhost:7279/api"; // URL pro API
            private readonly string _connectionString = "Data Source=parkingdb.db;";

            public ParkingController(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            
            public async Task<IActionResult> Index()
            {
            
                var parkingLotsResponse = await _httpClient.GetStringAsync($"{_baseApiUrl}/parkinglots");
                var parkingLots = JsonConvert.DeserializeObject<List<ParkingLot>>(parkingLotsResponse);

            
                var parkingSpotsResponse = await _httpClient.GetStringAsync($"{_baseApiUrl}/parkingspots");
                var parkingSpots = JsonConvert.DeserializeObject<List<ParkingSpot>>(parkingSpotsResponse);

            
                foreach (var lot in parkingLots)
                {
                    lot.ParkingSpots = parkingSpots.Where(s => s.ParkingLotId == lot.Id).ToList();
                }

                return View(parkingLots);  
            }



            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> SpotDetail(int parkingLotId)
            {
                // Načti parkoviště
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/parkinglots/{parkingLotId}");
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var parkingLot = JsonConvert.DeserializeObject<ParkingLot>(json);

                // Načti statistiky
                var statsResponse = await _httpClient.GetAsync($"{_baseApiUrl}/parkinglots/{parkingLotId}/stats");
                if (statsResponse.IsSuccessStatusCode)
                {
                    using var doc = await JsonDocument.ParseAsync(await statsResponse.Content.ReadAsStreamAsync());
                    if (doc.RootElement.TryGetProperty("endedOccupationsLastMonth", out var value))
                    {
                        ViewBag.ParkingsLastMonth = value.GetInt32();
                    }
                    else
                    {
                        ViewBag.ParkingsLastMonth = 0;
                    }
                }
                else
                {
                    ViewBag.ParkingsLastMonth = 0;
                }

                return View(parkingLot);
            }
            [Authorize(Roles = "Admin")]
            [HttpGet]
            public async Task<IActionResult> AddParkingSpot(int parkingLotId)
            {
                var response = await _httpClient.PostAsync(
                    $"{_baseApiUrl}/parkinglots/{parkingLotId}/addspot",
                    null 
                );
                 return RedirectToAction("SpotDetail", new { parkingLotId });
           
           
            }

            [Authorize(Roles ="Admin")]
            [HttpPost]
            public async Task<IActionResult> RemoveParkingSpot(int spotId, int parkingLotId)
            {
                var response = await _httpClient.DeleteAsync(
                    $"{_baseApiUrl}/parkinglots/{parkingLotId}/removespot/{spotId}"
                );   
                    return RedirectToAction("SpotDetail", new { parkingLotId });
            
           
            }
            [HttpPost]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> SetSpotToMaintenance(int spotId, int parkingLotId)
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"{_baseApiUrl}/parkinglots/{parkingLotId}/spot/{spotId}/maintenance",
                    new { });

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("SpotDetail", new { parkingLotId });
                }

                ModelState.AddModelError("", "Došlo k chybě při převodu parkovacího místa na údržbu.");
                return RedirectToAction("SpotDetail", new { parkingLotId });
            }
            [HttpPost]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> SetSpotToAvailable(int spotId, int parkingLotId)
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"{_baseApiUrl}/parkinglots/{parkingLotId}/spot/{spotId}/available",
                    new { });

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("SpotDetail", new { parkingLotId });
                }

                ModelState.AddModelError("", "Došlo k chybě při převodu parkovacího místa na dostupný stav.");
                return RedirectToAction("SpotDetail", new { parkingLotId });
            }

            [Authorize(Roles ="Admin")]
            [HttpGet]
            public async Task<IActionResult> SpotHistory(int spotId)
            {
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/parkingspots/{spotId}/history");

                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var json = await response.Content.ReadAsStringAsync();
                var history = JsonConvert.DeserializeObject<List<ParkingSpotHistory>>(json);

                return View(history);
            }

       




            [Authorize]
            [HttpGet]
            public IActionResult Occupy(int parkingLotId)
            {
           
                ViewBag.ParkingLotId = parkingLotId;
                return View(); 
            }

            [Authorize]
            [HttpPost]
            public async Task<IActionResult> OccupyParkingSpot(int parkingLotId, ParkingOccupation model)
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

            
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Nepodařilo se zjistit ID uživatele.");
                }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Zadejte SPZ ve validním formátu";
                return RedirectToAction("Occupy", new { parkingLotId = parkingLotId });
            }
            var spot = await connection.QueryFirstOrDefaultAsync<ParkingSpot>(
                    "SELECT * FROM ParkingSpots WHERE ParkingLotId = @ParkingLotId AND Status = @Status",
                    new { ParkingLotId = parkingLotId, Status = ParkingSpotStatus.Available });

                if (spot == null)
                {
                    TempData["ErrorMessage"] = "Není k dispozici žádné volné parkovací místo.";
                    return RedirectToAction("Index", "Parking");
                }

                var oldStatus = spot.Status;
                spot.Status = ParkingSpotStatus.Occupied;

           
                await connection.ExecuteAsync(
                    "UPDATE ParkingSpots SET Status = @Status WHERE Id = @SpotId",
                    new { Status = spot.Status, SpotId = spot.Id });

            
                await connection.ExecuteAsync(
                    "INSERT INTO ParkingSpotHistory (ParkingSpotId, OldStatus, NewStatus, ChangedAt) VALUES (@ParkingSpotId, @OldStatus, @NewStatus, @ChangedAt)",
                    new
                    {
                        ParkingSpotId = spot.Id,
                        OldStatus = oldStatus,
                        NewStatus = spot.Status,
                        ChangedAt = DateTime.UtcNow
                    });

           
                await connection.ExecuteAsync(
                    "INSERT INTO ParkingOccupations (ParkingSpotId, LicensePlate, StartTime, UserId) VALUES (@ParkingSpotId, @LicensePlate, @StartTime, @UserId)",
                    new
                    {
                        ParkingSpotId = spot.Id,
                        LicensePlate = model.LicensePlate, 
                        StartTime = DateTime.UtcNow,
                        UserId = userId
                    });

                TempData["SuccessMessage"] = $"Místo {spot.Id} bylo úspěšně obsazeno.";
                return RedirectToAction("Index");
            }

            [Authorize]
            public async Task<IActionResult> MyOccupations()
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    TempData["ErrorMessage"] = "Uživatel není přihlášen.";
                    return RedirectToAction("Index", "Home");
                }

                using var connection = new SqliteConnection(_connectionString);
                var occupations = await connection.QueryAsync<ParkingOccupation>(
                    "SELECT * FROM ParkingOccupations WHERE UserId = @UserId AND EndTime IS NULL",
                    new { UserId = userId });

                return View(occupations);
            }
            [Authorize]
            [HttpPost]
            public async Task<IActionResult> EndOccupancy(int parkingSpotId)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    TempData["ErrorMessage"] = "Nepodařilo se zjistit ID uživatele.";
                    return RedirectToAction("MyOccupations");
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var occupation = await connection.QueryFirstOrDefaultAsync<ParkingOccupation>(
                    "SELECT * FROM ParkingOccupations WHERE ParkingSpotId = @SpotId AND UserId = @UserId AND EndTime IS NULL",
                    new { SpotId = parkingSpotId, UserId = userId });

                if (occupation == null)
                {
                    TempData["ErrorMessage"] = "Nepodařilo se najít aktivní obsazení.";
                    return RedirectToAction("MyOccupations");
                }

                var endTime = DateTime.UtcNow;
                var duration = endTime - occupation.StartTime;

            
                var totalHours = Math.Ceiling(duration.TotalHours);
                var price = totalHours * 30; // 30 Kč/hod

                await connection.ExecuteAsync(
                    "UPDATE ParkingOccupations SET EndTime = @EndTime WHERE Id = @Id",
                    new { EndTime = endTime, Id = occupation.Id });

                await connection.ExecuteAsync(
                    "UPDATE ParkingSpots SET Status = @Status WHERE Id = @SpotId",
                    new { Status = ParkingSpotStatus.Available, SpotId = parkingSpotId });

                await connection.ExecuteAsync(
                    "INSERT INTO ParkingSpotHistory (ParkingSpotId, OldStatus, NewStatus, ChangedAt) VALUES (@SpotId, @OldStatus, @NewStatus, @ChangedAt)",
                    new
                    {
                        SpotId = parkingSpotId,
                        OldStatus = ParkingSpotStatus.Occupied,
                        NewStatus = ParkingSpotStatus.Available,
                        ChangedAt = endTime
                    });

                TempData["SuccessMessage"] = $"Obsazení místa {parkingSpotId} bylo ukončeno. " +
                                             $"Zaplaťte prosím {price} Kč v automatu u vjezdu.";

                return RedirectToAction("MyOccupations");
            }




        }
    }
