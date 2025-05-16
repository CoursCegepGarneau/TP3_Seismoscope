using Moq;
using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeismoscopeTest.Utils
{
    public class CsvUtilesTests
    {
        [Fact]
        public void LireLecturesDepuisCsv_ShouldParseValidCsvAndCallAdjustSensors()
        {
            // Arrange
            var csvContent = "TypeOnde,Amplitude,Note\nP,12.5,Note1\nS,20.7,Note2";
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, csvContent);

            var mockAdjustementService = new Mock<ISensorAdjustementService>();

            var sensor = new Sensor { Id = 1, Frequency = 10 };

            // Act
            var result = CsvUtils.LireLecturesDepuisCsv(filePath, sensor, mockAdjustementService.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal("P", result[0].TypeOnde);
            Assert.Equal(12.5, result[0].Amplitude);
            Assert.Equal("Note1", result[0].Note);

            Assert.Equal("S", result[1].TypeOnde);
            Assert.Equal(20.7, result[1].Amplitude);
            Assert.Equal("Note2", result[1].Note);

            mockAdjustementService.Verify(s => s.AdjustSensors(It.IsAny<SeismicEvent>(), sensor), Times.Exactly(2));

            File.Delete(filePath);
        }

        [Fact]
        public void LireLecturesDepuisCsv_ShouldReturnEmptyList_IfFileDoesNotExist()
        {
            // Arrange
            var invalidPath = "invalid.csv";
            var mockAdjustementService = new Mock<ISensorAdjustementService>();
            var sensor = new Sensor();

            // Act
            var result = CsvUtils.LireLecturesDepuisCsv(invalidPath, sensor, mockAdjustementService.Object);

            // Assert
            Assert.Empty(result);
            mockAdjustementService.Verify(s => s.AdjustSensors(It.IsAny<SeismicEvent>(), It.IsAny<Sensor>()), Times.Never);
        }
    }
}
