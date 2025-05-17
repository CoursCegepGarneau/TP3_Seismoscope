using Seismoscope.Model;
using System.Collections.Generic;

namespace Seismoscope.Utils.Services.Interfaces
{
    public interface IStationService
    {
        IEnumerable<Station> GetAllStations();
        Station? GetStationById(int id);
        void AddStation(Station station);
        void UpdateStation(Station station);
        void DeleteStation(int id);
    }
}
