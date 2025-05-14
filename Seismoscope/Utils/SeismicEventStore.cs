using Seismoscope.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Utils
{
    public class SeismicEventStore
    {
        private static readonly Queue<SeismicEvent> _lastSeismicEvents = new();
        private const int MAX_SEISMIC_EVENTS_QUEUE_LENGTH = 10;

        public static IReadOnlyCollection<SeismicEvent> LastSeismicEvents => _lastSeismicEvents.ToList();

        public static void AddEvent(SeismicEvent newSeismicEvent)
        {
            if (_lastSeismicEvents.Count >= MAX_SEISMIC_EVENTS_QUEUE_LENGTH)
                _lastSeismicEvents.Dequeue();

            _lastSeismicEvents.Enqueue(newSeismicEvent);
        }

        public static bool IsEmpty => !_lastSeismicEvents.Any();
    }
}
