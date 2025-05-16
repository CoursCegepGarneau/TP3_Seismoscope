using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Seismoscope.Utils.Services;

namespace SeismoscopeTest.Utils.Services
{
    public class ReaderServiceTests
    {
        [Fact]
        public void LoadCsv_ShouldLoadLinesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var lines = new[] { "Ligne1", "Ligne2", "Ligne3" };
            File.WriteAllLines(tempFile, lines);

            var service = new ReaderService();

            // Act
            service.LoadCsv(tempFile);

            // Assert
            Assert.Equal(lines.Length, service.GetTotalLines());
            Assert.Equal("Ligne1", service.GetNextLine(0));
            Assert.Equal("Ligne3", service.GetNextLine(2));
            Assert.Null(service.GetNextLine(3));

            // Cleanup
            File.Delete(tempFile);
        }
    }

}
