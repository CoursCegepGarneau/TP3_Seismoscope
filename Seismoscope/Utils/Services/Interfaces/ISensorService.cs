namespace Seismoscope.Model.Interfaces
{
    public interface ISensorService
    {
        IList<Sensor> GetAllSensors();
        Sensor? GetSensorById(int id);
        IList<Sensor> GetSensorByStationId(int stationId);
        void ActivateSensor(Sensor sensor);
        void DeactivateSensor(Sensor sensor);
        void ChangeSensorFrequency(Sensor sensor);
        void ChangeSensorTreshold(Sensor sensor);
        void AddSensor(Sensor sensor);
        void DeleteSensor(Sensor sensor);
        void UpdateSensor(Sensor sensor);
        void UpdateSensorStatus(Sensor sensor);
        void UpdateSensorOperationalStatus(Sensor sensor);
        void UpdateSensorDeliveryStatus(Sensor sensor);
    }
}
