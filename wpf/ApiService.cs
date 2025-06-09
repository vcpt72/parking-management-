using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ParkingWPF
{
    public class ApiService : IDisposable
    {
        private readonly HttpClient _httpClient;

        public ApiService(string baseUrl)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<List<ParkingLot>> GetParkingLotsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ParkingLot>>("parkinglots");
        }

        public async Task<ParkingLot> GetParkingLotByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ParkingLot>($"parkinglots/{id}");
        }

        public async Task<int> GetEndedOccupationsLastMonthAsync(int parkingLotId)
        {
            var response = await _httpClient.GetFromJsonAsync<ParkingStatsResponse>($"parkinglots/{parkingLotId}/stats");
            return response?.EndedOccupationsLastMonth ?? 0;
        }

        public async Task<bool> AddParkingSpotAsync(int parkingLotId, ParkingSpot spot)
        {
            var response = await _httpClient.PostAsJsonAsync($"parkinglots/{parkingLotId}/addspot", spot);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveParkingSpotAsync(int parkingLotId, int spotId)
        {
            var response = await _httpClient.DeleteAsync($"parkinglots/{parkingLotId}/removespot/{spotId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetParkingSpotToMaintenanceAsync(int parkingLotId, int spotId)
        {
            var response = await _httpClient.PutAsync($"parkinglots/{parkingLotId}/spot/{spotId}/maintenance", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SetParkingSpotToAvailableAsync(int parkingLotId, int spotId)
        {
            var response = await _httpClient.PutAsync($"parkinglots/{parkingLotId}/spot/{spotId}/available", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ParkingSpotHistory>> GetParkingSpotHistoryAsync(int spotId)
        {
            return await _httpClient.GetFromJsonAsync<List<ParkingSpotHistory>>($"parkingspots/{spotId}/history");
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
