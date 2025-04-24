using Seismoscope.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Data.Repositories.Interfaces
{
    public interface ISensorRepository
    {
        IList<Sensor> GetAll();
        Sensor GetById(int id);
        IList<Sensor> GetByStationId(int stationId);
        void ChangeFrequency(Sensor sensor);
        void ChangeTreshold(Sensor sensor);
        void AddSensor(Sensor sensor);
        void DeleteSensor(Sensor sensor);
        void UpdateSensor(Sensor sensor);
        void UpdateSensorStatus(Sensor sensor);
        void UpdateOperationalStatus(Sensor sensor);
        void UpdateDeliveryStatus(Sensor sensor);
    }
}
