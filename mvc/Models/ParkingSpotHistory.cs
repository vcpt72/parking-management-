namespace Parking_projekt.Models
{
    public class ParkingSpotHistory
    {
        public int Id { get; set; }          
        public int ParkingSpotId { get; set; }  
        public ParkingSpotStatus OldStatus { get; set; }  
        public ParkingSpotStatus NewStatus { get; set; } 
        public DateTime ChangedAt { get; set; } 

    }
}
