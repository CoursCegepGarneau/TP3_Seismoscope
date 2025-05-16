using Seismoscope.Model;
using Seismoscope.Utils.Services;
using System.Collections.Generic;
using Xunit;

namespace SeismoscopeTest.Utils.Services
{
    public class SeismicEventStoreServicesTests
    {
        [Fact]
        public void AddEvent_ShouldStoreEvent()
        {
            // Arrange
            var store = new SeismicEventStoreService();
            var seismicEvent = new SeismicEvent { Amplitude = 3.5 };

            // Act
            store.AddEvent(seismicEvent);
            var events = store.GetLastSeismicEvents();

            // Assert
            Assert.Single(events);
            Assert.Contains(seismicEvent, events);
        }

        [Fact]
        public void AddEvent_ShouldRemoveOldest_WhenCapacityExceeded()
        {
            // Arrange
            var store = new SeismicEventStoreService();
            var initialEvents = new List<SeismicEvent>();

            for (int i = 0; i < 10; i++)
            {
                var ev = new SeismicEvent { Amplitude = i };
                initialEvents.Add(ev);
                store.AddEvent(ev);
            }

            var overflowEvent = new SeismicEvent { Amplitude = 99 };

            // Act
            store.AddEvent(overflowEvent);
            var events = store.GetLastSeismicEvents();

            // Assert
            Assert.Equal(10, events.Count);
            Assert.DoesNotContain(initialEvents[0], events);
            Assert.Contains(overflowEvent, events);
        }

        [Fact]
        public void GetLastSeismicEvents_ShouldReturnCopy()
        {
            // Arrange
            var store = new SeismicEventStoreService();
            store.AddEvent(new SeismicEvent { Amplitude = 1.0 });

            // Act
            var events = store.GetLastSeismicEvents();
            Assert.IsAssignableFrom<IReadOnlyCollection<SeismicEvent>>(events);

            var list = (List<SeismicEvent>)events;
            list.Clear();

            // Assert
            Assert.Single(store.GetLastSeismicEvents());
        }
    }
}
