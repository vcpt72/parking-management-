using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingWPF
{
    public enum ParkingSpotStatus
    {
        Available = 0,
        Occupied = 1,
        Maintenance = 2
    }
    public class ParkingSpot // parkovaci misto
    {

        public int Id { get; set; }  // id v dbs
        public int ParkingLotId { get; set; }

        public ParkingSpotStatus Status { get; set; }
        public ParkingOccupation? CurrentOccupation { get; set; }
        public int LocalId { get; set; } // id v parkovisti
        public List<ParkingSpotHistory> StatusHistory { get; set; }

    }
}
