using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Model
{
    public class Station
    {
        public int Id { get; set; } // Clé primaire
        public string Nom { get; set; }
        public string? Région { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // liste des employés liés à cette station (optionnel)
        public ICollection<User>? Employes { get; set; }
    }

}
