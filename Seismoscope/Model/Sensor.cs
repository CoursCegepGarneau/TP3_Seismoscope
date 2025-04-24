using Seismoscope.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Model
{
    public class Sensor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Frequency { get; set; }
        public double Treshold { get; set; }
        public bool Delivered { get; set; } 
        public bool Operational {  get; set; } 
        public bool SensorStatus {  get; set; } // Activé ou désactivé
        public SensorUsage Usage { get; set;} //Enum: "Disponible", "Assigné", etc.
        public Station? assignedStation { get; set; }

        public int AssignedStationId;


    }
}
