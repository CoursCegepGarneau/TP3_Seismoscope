using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Model
{
    public class HistoriqueEvenement
    {
        public int Id { get; set; }
        public DateTime DateHeure { get; set; }


        public string TypeOnde { get; set; } = "";
        public double Amplitude { get; set; }
        public string Note { get; set; } = "";


        public double SeuilAuMoment { get; set; }
        public int SensorId { get; set; }
        public string SensorName { get; set; }

        public Sensor? Sensor { get; set; }

        // Optionnel : pour traitement plus facile
        public List<string> Règles => ParseRègles(Note);

        private List<string> ParseRègles(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return new();
            return note
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => r.StartsWith("Règle")) // filtre les vraies règles
                .ToList();
        }


        
        
        
    }
}
