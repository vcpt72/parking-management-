using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingWPF
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
