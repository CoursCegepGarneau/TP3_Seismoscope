using Seismoscope.Data;
using Seismoscope.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Seismoscope.Model
{
    public class StationRepository : IStationRepository
    {
        private readonly ApplicationDbContext _context;

        public StationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Station> GetAll()
        {
            return _context.Stations.ToList();
        }

        public Station? GetById(int id)
        {
            return _context.Stations.FirstOrDefault(s => s.Id == id);
        }

        public void Add(Station station)
        {
            _context.Stations.Add(station);
            _context.SaveChanges();
        }

        public void Update(Station station)
        {
            _context.Stations.Update(station);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var station = _context.Stations.Find(id);
            if (station != null)
            {
                _context.Stations.Remove(station);
                _context.SaveChanges();
            }
        }
    }
}
