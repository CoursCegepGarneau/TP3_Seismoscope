using Seismoscope.Model;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Services.Interfaces;
using NLog;

namespace Seismoscope.Utils
{
    public static class CsvUtils
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public static List<SeismicEvent> LireLecturesDepuisCsv(string cheminFichier)
        {
            var lectures = new List<SeismicEvent>();

            if (!File.Exists(cheminFichier))
            {
                logger.Warn($"Le fichier CSV '{cheminFichier}' est introuvable.");
                return lectures;
            }

            logger.Info($"Début de lecture du fichier CSV : {cheminFichier}");

            try
            {
                var lignes = File.ReadAllLines(cheminFichier);

                for (int i = 1; i < lignes.Length; i++) // Ignorer l'en-tête
                {
                    var ligne = lignes[i];
                    var colonnes = ligne.Split(',');

                    if (colonnes.Length < 2)
                    {
                        logger.Warn($"Ligne {i + 1} ignorée (colonnes insuffisantes)");
                        continue;
                    }

                    if (!double.TryParse(colonnes[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double amplitude))
                    {
                        logger.Warn($"Ligne {i + 1} ignorée (amplitude invalide : '{colonnes[1]}')");
                        continue;
                    }

                    var newEvent = new SeismicEvent
                    {
                        TypeOnde = colonnes[0].Trim(),
                        Amplitude = amplitude,
                        Note = colonnes.Length >= 3 ? colonnes[2].Trim() : null,
                        Timestamp = DateTime.Now
                    };

                    lectures.Add(newEvent);
                }

                logger.Info($"Fin de lecture du fichier CSV — {lectures.Count} lectures valides extraites.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Erreur lors de la lecture du fichier CSV : {cheminFichier}");
            }

            return lectures;
        }


        /*public static List<SeismicEvent> LireLecturesDepuisCsv(string cheminFichier, Sensor selectedSensor, ISensorAdjustementService sensorAdjustementService, out List<string> messages)
        {
            var lectures = new List<SeismicEvent>();
            messages = new List<string>();

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

                var ruleMessages = sensorAdjustementService.AdjustSensors(newEvent, selectedSensor);
                messages.AddRange(ruleMessages);

                lectures.Add(newEvent);
            }

            return lectures;
        }
    }
*/



    }
}