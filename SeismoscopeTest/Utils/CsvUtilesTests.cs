using Moq;
using Seismoscope.Model;
using Seismoscope.Utils.Services.Interfaces;
using Seismoscope.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seismoscope.ViewModel;

namespace SeismoscopeTest.Utils
{
    public class CsvUtilesTests
    {
        /*[Fact]
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


        [Fact]
        public async Task StartReadingAsync_ShouldApplyAdjustmentsAndAddMessages()
        {
            // Arrange
            var sensor = new Sensor { Id = 1, Name = "TestSensor", Treshold = 50, Frequency = 1 };
            var viewModel = new SensorReadingViewModel(...); // Injecte _sensorAdjustementService mock ici

            var mockAdjustService = new Mock<ISensorAdjustementService>();
            mockAdjustService
                .Setup(s => s.AdjustSensors(It.IsAny<SeismicEvent>(), sensor))
                .Returns<SeismicEvent, Sensor>((evt, s) =>
                {
                    var messages = new List<string>();
                    if (evt.Amplitude > s.Treshold * 1.3)
                    {
                        s.Treshold *= 1.1;
                        messages.Add("🔧 Seuil ajusté");
                    }
                    if (evt.Amplitude > s.Treshold * 1.6)
                    {
                        s.Frequency *= 0.9;
                        messages.Add("⚡ Fréquence ajustée");
                    }
                    return messages;
                });

            viewModel.SelectedSensor = sensor;
            viewModel._donneesSismiques = new List<SeismicEvent>
    {
        new SeismicEvent { Amplitude = 85, TypeOnde = "P" }, // > 160%
        new SeismicEvent { Amplitude = 70, TypeOnde = "S" }, // > 130%
        new SeismicEvent { Amplitude = 40, TypeOnde = "P" }  // < seuil
    };

            // Act
            await viewModel.StartReadingAsync();

            // Assert
            Assert.True(viewModel.MessagesUI.Any(m => m.Contains("Seuil") || m.Contains("Fréquence")));
            Assert.True(sensor.Treshold > 50);  // Vérifie si seuil ajusté
            Assert.True(sensor.Frequency < 1);  // Vérifie si fréquence ajustée
        }*/

        [Fact]
        public void LireLecturesDepuisCsv_ShouldParseValidCsvCorrectly()
        {
            // Arrange
            var csvContent = "TypeOnde,Amplitude,Note\nP,12.5,Note1\nS,20.7,Note2";
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, csvContent);

            // Act
            var result = CsvUtils.LireLecturesDepuisCsv(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal("P", result[0].TypeOnde);
            Assert.Equal(12.5, result[0].Amplitude);
            Assert.Equal("Note1", result[0].Note);

            Assert.Equal("S", result[1].TypeOnde);
            Assert.Equal(20.7, result[1].Amplitude);
            Assert.Equal("Note2", result[1].Note);

            File.Delete(filePath); // Nettoyage
        }

        [Fact]
        public void LireLecturesDepuisCsv_ShouldReturnEmptyList_IfFileDoesNotExist()
        {
            // Arrange
            var invalidPath = "nonexistent_file.csv";

            // Act
            var result = CsvUtils.LireLecturesDepuisCsv(invalidPath);

            // Assert
            Assert.NotNull(result); // Doit retourner une liste vide, pas null
            Assert.Empty(result);
        }






    }


}
