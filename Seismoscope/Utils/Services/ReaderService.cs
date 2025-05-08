using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Utils.Services
{
    public class ReaderService
    {
        private List<string> _csvLines = new();

        public void LoadCsv(string path)
        {
            if (File.Exists(path))
            {
                _csvLines = new List<string>(File.ReadAllLines(path));
            }
        }

        public string? GetNextLine(int index)
        {
            return index < _csvLines.Count ? _csvLines[index] : null;
        }

        public int GetTotalLines()
        {
            return _csvLines.Count;
        }
    }
}
