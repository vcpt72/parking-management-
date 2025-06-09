using Dapper;
using Microsoft.Data.Sqlite;

namespace Parking_projekt
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateTables()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var sql = @"
            CREATE TABLE IF NOT EXISTS ParkingLots (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Latitude REAL NOT NULL,
                Longitude REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ParkingSpots (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ParkingLotId INTEGER NOT NULL,
                Status INTEGER NOT NULL,
                FOREIGN KEY (ParkingLotId) REFERENCES ParkingLots(Id)
            );

            CREATE TABLE IF NOT EXISTS ParkingSpotHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ParkingSpotId INTEGER NOT NULL,
                OldStatus INTEGER NOT NULL,
                NewStatus INTEGER NOT NULL,
                ChangedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (ParkingSpotId) REFERENCES ParkingSpots(Id)
            );

            CREATE TABLE IF NOT EXISTS ParkingOccupations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ParkingSpotId INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                LicensePlate TEXT NOT NULL,
                StartTime DATETIME NOT NULL,
                EndTime DATETIME,
                Price REAL,
                FOREIGN KEY (ParkingSpotId) REFERENCES ParkingSpots(Id),
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Role INTEGER NOT NULL
            );
            ";

            connection.Execute(sql);
        }
    }
}
