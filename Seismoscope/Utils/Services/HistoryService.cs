using Seismoscope.Model.Interfaces;
using Seismoscope.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;
using Seismoscope.Utils.Services.Interfaces;

namespace Seismoscope.Model.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoriqueRepository _historiqueRepository;

        public HistoryService(IHistoriqueRepository historiqueRepository)
        {
            _historiqueRepository = historiqueRepository;
        }


        
        void IHistoryService.AjouterHistory(HistoriqueEvenement evenement)
        {
            _historiqueRepository.Ajouter(evenement);
        }

        IList<HistoriqueEvenement> IHistoryService.GetAllHistory()
        {
            return _historiqueRepository.GetAll();
        }

        IList<HistoriqueEvenement> IHistoryService.GetHistoryBySensor(int sensorId)
        {
            return _historiqueRepository.GetBySensor(sensorId);
        }

        IList<HistoriqueEvenement> IHistoryService.FiltrerHistoryParTypeOnde(string typeOnde)
        {
            throw new NotImplementedException();
        }
    }
}
