using System.Collections.Generic;
using System.Windows;

namespace ParkingWPF
{
    public partial class ParkingSpotHistoryPage : Window
    {
        private readonly int _spotId;
        private readonly ApiService _apiService = new ApiService("https://localhost:7279/api/");

        public ParkingSpotHistoryPage(int spotId)
        {
            InitializeComponent();
            _spotId = spotId;
            LoadHistory();
        }

        private async void LoadHistory()
        {
            var history = await _apiService.GetParkingSpotHistoryAsync(_spotId);
            HistoryGrid.ItemsSource = history;
        }
    }
}
