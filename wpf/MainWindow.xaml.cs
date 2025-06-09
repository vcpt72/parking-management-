using System.Windows;

namespace ParkingWPF
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService = new ApiService("https://localhost:7279/api/");

        public MainWindow()
        {
            InitializeComponent();
            LoadParkingLots();
        }

        private async void LoadParkingLots()
        {
            
                var lots = await _apiService.GetParkingLotsAsync();
                ParkingLotsList.ItemsSource = lots;
            
           
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ParkingLotsList.SelectedItem is ParkingLot selectedLot)
            {
                var detailWindow = new ParkingLotDetailWindow(selectedLot.Id);
                detailWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vyber prosím parkoviště ze seznamu.");
            }
        }
    }
}
