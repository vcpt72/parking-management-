using System.ComponentModel.DataAnnotations;

namespace Parking_projekt.Models
{
	public class ParkingOccupation // obsazenost parkovaciho mista
	{

		public int Id { get; set; }
		public int ParkingSpotId { get; set; } // FK na parkovaci misto
        public int UserId { get; set; }

        [RegularExpression(@"^[A-Z0-9]{3}\s?[A-Z0-9]{4}$")]
        public string LicensePlate { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }



		public double? price { get; set; }

	}
}
