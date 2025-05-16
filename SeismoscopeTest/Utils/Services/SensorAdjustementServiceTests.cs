using Moq;
using Seismoscope.Model;
using Seismoscope.Utils.Services;
using Seismoscope.Utils.Services.Interfaces;
using System.Collections.Generic;
using Xunit;

namespace SeismoscopeTest.Utils.Services
{
    public class SensorAdjustementServiceTests
    {
        [Fact]
        public void AdjustSensors_ShouldIncreaseThreshold_WhenAmplitudeAbove130Percent()
        {
            // Arrange
            var mockStore = new Mock<ISeismicEventStoreService>();
            mockStore.Setup(s => s.GetLastSeismicEvents())
                     .Returns(new List<SeismicEvent>());

            var service = new SensorAdjustementService(mockStore.Object);

            var sensor = new Sensor
            {
                Treshold = 10,
                MaxThreshold = 20,
                MinThreshold = 5,
                Frequency = 5,
                DefaultFrequency = 5,
                MaxFrequency = 1
            };

            var seismicEvent = new SeismicEvent
            {
                Amplitude = 14
            };

            // Act
            service.AdjustSensors(seismicEvent, sensor);

            // Assert
            Assert.Equal(11, sensor.Treshold);
            mockStore.Verify(s => s.AddEvent(seismicEvent), Times.Once);
        }

        [Fact]
        public void AdjustSensors_ShouldLowerTreshold_WhenAmplitudeHigherThan80PercentAndLowerThan100PercentForLast5Events()
        {
            // Arrange
            var mockStore = new Mock<ISeismicEventStoreService>();

            double initialThreshold = 10.0;
            double amplitude = 9.0;

            var lastEvents = new List<SeismicEvent>
            {
                new SeismicEvent { Amplitude = amplitude },
                new SeismicEvent { Amplitude = amplitude },
                new SeismicEvent { Amplitude = amplitude },
                new SeismicEvent { Amplitude = amplitude },
                new SeismicEvent { Amplitude = amplitude }
            };

            mockStore.Setup(s => s.GetLastSeismicEvents()).Returns(lastEvents);

            var sensor = new Sensor
            {
                Treshold = initialThreshold,
                MaxThreshold = 20,
                MinThreshold = 5,
                Frequency = 5,
                DefaultFrequency = 5,
                MaxFrequency = 1
            };

            var newEvent = new SeismicEvent { Amplitude = 5 };

            var service = new SensorAdjustementService(mockStore.Object);

            // Act
            service.AdjustSensors(newEvent, sensor);

            // Assert
            double expectedThreshold = initialThreshold * 0.9;
            Assert.Equal(expectedThreshold, sensor.Treshold, precision: 5);

            mockStore.Verify(s => s.AddEvent(newEvent), Times.Once);
        }

        [Fact]
        public void AdjustSensors_ShouldIncreaseFrequency_WhenAmplitudeHigerThan160PercentOfThreshold()
        {
            // Arrange
            var mockStore = new Mock<ISeismicEventStoreService>();

            mockStore.Setup(s => s.GetLastSeismicEvents()).Returns(new List<SeismicEvent>());

            double threshold = 10.0;
            double amplitude = 20.0;

            var sensor = new Sensor
            {
                Treshold = threshold,
                MaxThreshold = 20,
                MinThreshold = 5,
                Frequency = 10,
                MaxFrequency = 5,
                DefaultFrequency = 10
            };

            double originalFrequency = sensor.Frequency * 1.1;

            var newEvent = new SeismicEvent { Amplitude = amplitude };

            var service = new SensorAdjustementService(mockStore.Object);

            // Act
            service.AdjustSensors(newEvent, sensor);

            // Assert
            double expectedFrequency = Math.Max(Math.Round(originalFrequency * 0.9), sensor.MaxFrequency);
            Assert.Equal(expectedFrequency, sensor.Frequency);

            mockStore.Verify(s => s.AddEvent(newEvent), Times.Once);
        }

        [Fact]
        public void AdjustSensors_ShouldResetFrequency_WhenNoRecentActivity()
        {
            // Arrange
            var mockStore = new Mock<ISeismicEventStoreService>();
            mockStore.Setup(s => s.GetLastSeismicEvents())
                     .Returns(new List<SeismicEvent> { new(), new(), new(), new(), new(),
                                                       new(), new(), new(), new(), new() });

            var service = new SensorAdjustementService(mockStore.Object);

            var sensor = new Sensor
            {
                Treshold = 10,
                MaxThreshold = 20,
                MinThreshold = 5,
                Frequency = 2,
                DefaultFrequency = 5,
                MaxFrequency = 1
            };

            var seismicEvent = new SeismicEvent
            {
                Amplitude = 8
            };

            // Act
            service.AdjustSensors(seismicEvent, sensor);

            // Assert
            Assert.Equal(5, sensor.Frequency);
        }
    }
}
