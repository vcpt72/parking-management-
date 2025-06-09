using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingWPF
{
    public class ParkingOccupation // obsazenost parkovaciho mista
    {

        public int Id { get; set; }
        public int ParkingSpotId { get; set; } // FK na parkovaci misto
        public int UserId { get; set; }
        public string LicensePlate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }



        public double? price { get; set; }

    }
}
