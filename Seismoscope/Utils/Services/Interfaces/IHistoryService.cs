using Seismoscope.Model;

namespace Seismoscope.Utils.Services.Interfaces
{
    public interface IHistoryService
    {
        void AjouterHistory(HistoriqueEvenement e);
        IList<HistoriqueEvenement> GetAllHistory();
        IList<HistoriqueEvenement> GetHistoryBySensor(int sensorId);
        IList<HistoriqueEvenement> FiltrerHistoryParTypeOnde(string typeOnde);
    }
}
