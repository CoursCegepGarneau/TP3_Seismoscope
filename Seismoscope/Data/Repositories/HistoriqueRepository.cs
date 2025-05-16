using Seismoscope.Data.Repositories.Interfaces;
using Seismoscope.Model;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Seismoscope.Model
{
    public class HistoriqueRepository : IHistoriqueRepository
    {
        private readonly ApplicationDbContext _context;

        public HistoriqueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Ajouter(HistoriqueEvenement e)
        {
            _context.Historiques.Add(e);
            _context.SaveChanges();
        }

        public IList<HistoriqueEvenement> GetAll() =>
            _context.Historiques.Include(h => h.Sensor).OrderByDescending(h => h.DateHeure).ToList();

        public IList<HistoriqueEvenement> GetBySensor(int sensorId) =>
            _context.Historiques.Where(h => h.SensorId == sensorId).ToList();

        public IList<HistoriqueEvenement> FiltrerParTypeOnde(string typeOnde) =>
            _context.Historiques.Where(h => h.TypeOnde == typeOnde).ToList();
    }

}
