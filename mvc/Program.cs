using Microsoft.Data.Sqlite;
using Dapper;
using Parking_projekt.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
namespace Parking_projekt
{
	public class Program
	{
		public static void Main(string[] args)
		{
            /*
             SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLite);
             string connectionString = "Data Source=parkingdb.db;";
             var dbInitializer = new DatabaseInitializer(connectionString);
             dbInitializer.CreateTables();  


             using SqliteConnection conn = new SqliteConnection(connectionString);
             conn.Open();

             *//*
            string connectionString = "Data Source=parkingdb.db;";
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // ID uživatele, pro kterého chceme získat obsazení (napø. zadané ruènì nebo získané z nìjaké autentizace)
            int userId = 2;  // Zmìò podle potøeby, nebo použij dynamické ID

            // SQL dotaz pro získání aktivního obsazení (synchronní verze)
            var occupations = connection.Query<ParkingOccupation>(
                "SELECT * FROM ParkingOccupations WHERE UserId = @UserId AND EndTime IS NULL",
                new { UserId = userId }).ToList();

            // Výpis do konzole
            if (occupations == null || !occupations.Any())
            {
                Console.WriteLine("No active parking occupations found for this user.");
            }
            else
            {
                foreach (var occupation in occupations)
                {
                    Console.WriteLine($"Parking Spot ----- ID: {occupation.ParkingSpotId}, userId{userId},  License Plate: {occupation.LicensePlate}, Start Time: {occupation.StartTime}");
                }
            }

            // Uzavøení pøipojení
            connection.Close();



            /*
             conn.Execute(@"
                 INSERT INTO Users (Username, Password, Role)
                 VALUES (@Username, @Password, @Role);
             ", new
             {
                 Username = "admin",
                 Password = "admin123",
                 Role = (int)UserRole.Admin // Role = Admin
             });


             var parkingLots = new[]
             {
                 new { Name = "VSB Parking Menza", Longitude = 18.1628143, Latitude = 49.8341084 },
                 new { Name = "VSB Parking Hala", Longitude = 18.1600355, Latitude = 49.8335894 },
                 new { Name = "VSB Parking Fei", Longitude = 18.1603789, Latitude = 49.8323091 }
             };

             foreach (var lot in parkingLots)
             {
                 var lotId = conn.ExecuteScalar<int>(
                     @"INSERT INTO ParkingLots (Name, Longitude, Latitude) 
               VALUES (@Name, @Longitude, @Latitude);
               SELECT last_insert_rowid();", lot);

                 // Pøidání 2 parkovacích míst pro každé parkovištì
                 for (int i = 1; i <= 2; i++)
                 {
                     conn.Execute(
                         @"INSERT INTO ParkingSpots (ParkingLotId, Status) 
                   VALUES (@ParkingLotId, @Status);",
                         new
                         {
                             ParkingLotId = lotId,
                             Status = 0 // 0 = Available
                         });
                 }
             }
            */

            /*
            var lots = conn.Query<ParkingLot>("SELECT * FROM ParkingLots").ToList();

            foreach (var lot in lots)
            {
                for (int i = 0; i < 2; i++) // 2 místa pro každé parkovištì
                {
                    conn.Execute(@"
            INSERT INTO ParkingSpots (ParkingLotId, Status)
            VALUES (@ParkingLotId, @Status)",
                        new
                        {
                            ParkingLotId = lot.Id,
                            Status = (int)ParkingSpotStatus.Available // = 0
                        });
                }
            }*/

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpClient();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
             {
                 options.LoginPath = "/api/Login";  // Cesta pro login
                 options.LogoutPath = "/api/Logout"; // Cesta pro logout
             });
            builder.Services.AddRazorPages();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            });




            // Add services to the container.
            builder.Services.AddControllersWithViews();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				
				app.UseHsts();
			}

            app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();


			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}


