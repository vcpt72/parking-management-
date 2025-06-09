using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ParkingWPF
{
    public partial class ParkingLotDetailWindow : Window
    {
        private readonly ApiService _apiService = new ApiService("https://localhost:7279/api/");
        private readonly int _parkingLotId;

        public ParkingLotDetailWindow(int parkingLotId)
        {
            InitializeComponent();
            _parkingLotId = parkingLotId;

            LoadParkingLotDetails();
        }

        private async void LoadParkingLotDetails()
        {
            var lot = await _apiService.GetParkingLotByIdAsync(_parkingLotId);
            if (lot == null)
            {
                MessageBox.Show("Nepodařilo se načíst detail parkoviště.");
                this.Close();
                return;
            }

            var ended = await _apiService.GetEndedOccupationsLastMonthAsync(_parkingLotId);

            NameText.Text = lot.Name;
            CoordinatesText.Text = $"Souřadnice: ({lot.Latitude}, {lot.Longitude})";
            EndedOccupationsText.Text = $"Za poslední měsíc bylo ukončeno parkování: {ended}";
            SpotsGrid.ItemsSource = lot.ParkingSpots;
        }
        private async void AddSpot_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Opravdu chcete přidat nové parkovací místo?", "Potvrzení", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var newSpot = new ParkingSpot
            {
                Status = ParkingSpotStatus.Available,
            };

            var success = await _apiService.AddParkingSpotAsync(_parkingLotId, newSpot);
            if (success)
            {
                MessageBox.Show("Místo přidáno.");
                LoadParkingLotDetails(); 
            }
            else
            {
                MessageBox.Show("Nepodařilo se přidat místo.");
            }

        }

        private async void DeleteSpot_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ParkingSpot spot)
            {
                var result = MessageBox.Show($"Opravdu chcete odstranit místo č. {spot.LocalId}?",
                                             "Potvrzení", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                var success = await _apiService.RemoveParkingSpotAsync(_parkingLotId, spot.Id);

                if (success)
                {
                    MessageBox.Show("Místo odstraněno.");
                    LoadParkingLotDetails(); 
                }
                else
                {
                    MessageBox.Show("Nepodařilo se odstranit místo.");
                }
            }
        }

        private async void SetToMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

           
            var parkingSpot = button.Tag as ParkingSpot;
            if (parkingSpot == null) return;

            var success = await _apiService.SetParkingSpotToMaintenanceAsync(parkingSpot.ParkingLotId, parkingSpot.Id);

            if (success)
            {
                MessageBox.Show("Stav parkovacího místa byl změněn na 'Maintenance'.");
                LoadParkingLotDetails();  
            }
            else
            {
                MessageBox.Show("Nepodařilo se změnit stav parkovacího místa.");
            }
        }

        
        private async void SetToAvailable_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

           
            var parkingSpot = button.Tag as ParkingSpot;
            if (parkingSpot == null) return;

            var success = await _apiService.SetParkingSpotToAvailableAsync(parkingSpot.ParkingLotId, parkingSpot.Id);

            if (success)
            {
                MessageBox.Show("Stav parkovacího místa byl změněn na 'Available'.");
                LoadParkingLotDetails(); 
            }
            else
            {
                MessageBox.Show("Nepodařilo se změnit stav parkovacího místa.");
            }
        }
        private void ShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var spot = (ParkingSpot)button.Tag;

            var historyPage = new ParkingSpotHistoryPage(spot.Id); 
            historyPage.Show(); 
        }
    }
}
