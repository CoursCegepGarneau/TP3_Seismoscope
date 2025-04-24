using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model.Interfaces;
using System.Collections.Generic;

namespace Seismoscope.Model.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;

        public StationService(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public IEnumerable<Station> GetAllStations()
        {
            return _stationRepository.GetAll();
        }

        public Station? GetStationById(int id)
        {
            return _stationRepository.GetById(id);
        }

        public void AddStation(Station station)
        {
            _stationRepository.Add(station);
        }

        public void UpdateStation(Station station)
        {
            _stationRepository.Update(station);
        }

        public void DeleteStation(int id)
        {
            _stationRepository.Delete(id);
        }
    }
}
