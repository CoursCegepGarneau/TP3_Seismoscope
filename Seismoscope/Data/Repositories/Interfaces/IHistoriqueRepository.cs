using Seismoscope.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Data.Repositories.Interfaces
{
    public interface IHistoriqueRepository
    {
        void Ajouter(HistoriqueEvenement e);
        IList<HistoriqueEvenement> GetAll();
        IList<HistoriqueEvenement> GetBySensor(int sensorId);
        IList<HistoriqueEvenement> FiltrerParTypeOnde(string typeOnde);
    }

}
