using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Seismoscope.Utils.Services
{
    public class SeismicEventStoreService : ISeismicEventStoreService
    {
        private readonly Queue<SeismicEvent> _lastSeismicEvents = new();
        private const int MAX_SEISMIC_EVENTS_QUEUE_LENGTH = 10;

        public void AddEvent(SeismicEvent newSeismicEvent)
        {
            if (_lastSeismicEvents.Count >= MAX_SEISMIC_EVENTS_QUEUE_LENGTH)
                _lastSeismicEvents.Dequeue();

            _lastSeismicEvents.Enqueue(newSeismicEvent);
        }

        public IReadOnlyCollection<SeismicEvent> GetLastSeismicEvents()
        {
            return _lastSeismicEvents.ToList();
        }
    }
}
