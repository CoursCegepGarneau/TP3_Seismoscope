using Seismoscope.Model;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Services.Interfaces;

namespace Seismoscope.Utils
{
    public static class CsvUtils
    {
        public static List<SeismicEvent> LireLecturesDepuisCsv(string cheminFichier, Sensor selectedSensor, ISensorAdjustementService sensorAdjustementService)
        {
            var lectures = new List<SeismicEvent>();

            if (!File.Exists(cheminFichier))
                return lectures;

            var lignes = File.ReadAllLines(cheminFichier);

            for (int i = 1; i < lignes.Length; i++) // Ignore la première ligne d'entête
            {
                var ligne = lignes[i];
                var colonnes = ligne.Split(',');

                if (colonnes.Length < 2)
                    continue; // Ligne invalide

                if (!double.TryParse(colonnes[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double amplitude))
                    continue; // Valeur non valide

                SeismicEvent newEvent = new SeismicEvent
                {
                    TypeOnde = colonnes[0].Trim(),
                    Amplitude = amplitude,
                    Note = colonnes.Length >= 3 ? colonnes[2].Trim() : null
                };

                sensorAdjustementService.AdjustSensors(newEvent, selectedSensor);

                lectures.Add(newEvent);
            }

            return lectures;
        }
    }
}
