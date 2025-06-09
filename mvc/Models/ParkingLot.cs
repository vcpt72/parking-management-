using Dapper;

namespace Parking_projekt.Models
{
	public class ParkingLot 
	{
        
		public int Id { get; set; }
		public string Name { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
        

        public List<ParkingSpot> ParkingSpots { get; set; }


    }

}
