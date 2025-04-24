using Seismoscope.Model;
using System.Collections.Generic;

namespace Seismoscope.Data.Repositories.Interfaces
{
    public interface IStationRepository
    {
        IEnumerable<Station> GetAll();
        Station? GetById(int id);
        void Add(Station station);
        void Update(Station station);
        void Delete(int id);
    }
}
