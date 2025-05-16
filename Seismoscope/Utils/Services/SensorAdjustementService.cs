using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Utils.Services
{
    public class SensorAdjustementService : ISensorAdjustementService
    {
        private readonly ISeismicEventStoreService _seismicEventStore;

        public SensorAdjustementService(ISeismicEventStoreService eventStore)
        {
            _seismicEventStore = eventStore;
        }

        public void AdjustSensors(SeismicEvent seismicEvent, Sensor sensor)
        {
            _seismicEventStore.AddEvent(seismicEvent);
            IReadOnlyCollection<SeismicEvent> lastEvents = _seismicEventStore.GetLastSeismicEvents();

            AdjustSensorHigherThreshold(seismicEvent, sensor);
            AdjustSensorLowerThreshold(sensor, lastEvents);
            AdjustSensorIncreaseFrequency(seismicEvent, sensor);
            AdjustSensorResetFrequency(sensor, lastEvents);
        }

        private void AdjustSensorHigherThreshold(SeismicEvent seismicEvent, Sensor sensor)
        {
            if (seismicEvent.Amplitude > sensor.Treshold * 1.3)
            {
                double newThreshold = sensor.Treshold * 1.1;
                sensor.Treshold = Math.Min(newThreshold, sensor.MaxThreshold);
            }
        }

        private void AdjustSensorLowerThreshold(Sensor sensor, IReadOnlyCollection<SeismicEvent> lastEvents)
        {
            var recentSensorEvents = lastEvents
                .TakeLast(5)
                .ToList();

            if (recentSensorEvents.Count < 5)
                return;

            bool match = recentSensorEvents
                .All(seismicEvent => seismicEvent.Amplitude >= sensor.Treshold * 0.8 && seismicEvent.Amplitude < sensor.Treshold);

            if (match)
            {
                double newThreshold = sensor.Treshold * 0.9;
                sensor.Treshold = Math.Max(newThreshold, sensor.MinThreshold);
            }
        }

        private void AdjustSensorIncreaseFrequency(SeismicEvent seismicEvent, Sensor sensor)
        {
            if (seismicEvent.Amplitude > sensor.Treshold * 1.6)
            {
                double newFrequency = sensor.Frequency * 0.9;
                sensor.Frequency = Math.Max(newFrequency, sensor.MaxFrequency);
            }
        }

        public void AdjustSensorResetFrequency(Sensor sensor, IReadOnlyCollection<SeismicEvent> lastEvents)
        {
            var recentSensorEvents = lastEvents.ToList();

            if (recentSensorEvents.Count == 0)
            {
                sensor.Frequency = sensor.DefaultFrequency;
                return;
            }

            double timeSinceLastEvent = recentSensorEvents.Count * sensor.Frequency;

            bool noRecentActivity = timeSinceLastEvent >= 120 ||recentSensorEvents.Count >= 10;

            if (noRecentActivity && sensor.Frequency < sensor.DefaultFrequency)
            {
                sensor.Frequency = sensor.DefaultFrequency;
            }
        }

    }
}
