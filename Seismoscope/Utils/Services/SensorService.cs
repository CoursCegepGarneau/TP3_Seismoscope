using Seismoscope.Model.Interfaces;
using Seismoscope.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace Seismoscope.Model.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _sensorRepository;

        public SensorService(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public IList<Sensor> GetAllSensors()
        {
            return _sensorRepository.GetAll();
        }

        public Sensor? GetSensorById(int id)
        {
            return _sensorRepository.GetById(id);
        }

        public IList<Sensor> GetSensorByStationId(int stationId)
        {
            return _sensorRepository.GetByStationId(stationId);
        }

        public void ActivateSensor(Sensor sensor)
        {
            sensor.SensorStatus = true;
            _sensorRepository.UpdateSensorStatus(sensor);
        }

        public void DeactivateSensor(Sensor sensor)
        {
            sensor.SensorStatus = false;
            _sensorRepository.UpdateSensorStatus(sensor);
        }

        public void ChangeSensorFrequency(Sensor sensor)
        {
            _sensorRepository.ChangeFrequency(sensor);
        }

        public void ChangeSensorTreshold(Sensor sensor)
        {
            _sensorRepository.ChangeTreshold(sensor);
        }

        public void AddSensor(Sensor sensor)
        {
            _sensorRepository.AddSensor(sensor);
        }

        public void DeleteSensor(Sensor sensor)
        {
            _sensorRepository.DeleteSensor(sensor);
        }

        public void UpdateSensor(Sensor sensor)
        {
            _sensorRepository.UpdateSensor(sensor);

        }

        public void UpdateSensorStatus(Sensor sensor)
        {
            _sensorRepository.UpdateSensorStatus(sensor);
        }

        public void UpdateSensorOperationalStatus(Sensor sensor)
        {
            _sensorRepository.UpdateOperationalStatus(sensor);
        }

        public void UpdateSensorDeliveryStatus(Sensor sensor)
        {
            _sensorRepository.UpdateDeliveryStatus(sensor);
        }
    }
}
