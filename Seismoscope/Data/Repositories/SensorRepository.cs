using Seismoscope.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Seismoscope.Model
{
    public class SensorRepository : ISensorRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public SensorRepository(ApplicationDbContext context)
        {
            _dbContext = context;
        }
        public IList<Sensor> GetAll()
        {
            return _dbContext.Sensors.ToList();
        }

        public IList<Sensor> GetAllIncludingStation()
        {
            return _dbContext.Sensors
                .Include(s => s.assignedStation)
                .ToList();
        }

        public Sensor GetById(int id)
        {
            return _dbContext.Sensors.FirstOrDefault(s => s.Id == id);
        }

        public IList<Sensor> GetByStationId(int stationId)
        {
            return _dbContext.Sensors
                .Include(s => s.assignedStation)
                .Where(s => s.assignedStation != null && s.assignedStation.Id == stationId)
                .ToList();
        }


        public void ChangeFrequency(Sensor sensor)
        {
            Sensor existingSensor = _dbContext.Sensors.FirstOrDefault(s => s.Id == sensor.Id);
            if (existingSensor != null)
            {
                existingSensor.Frequency = sensor.Frequency;
                _dbContext.SaveChanges();
            }
        }


        public void ChangeTreshold(Sensor sensor)
        {
            Sensor existingSensor = _dbContext.Sensors.FirstOrDefault(s => s.Id == sensor.Id);
            if (existingSensor != null)
            {
                existingSensor.Treshold = sensor.Treshold;
                _dbContext.SaveChanges();
            }
        }

        public ObservableCollection<Sensor> GetSensorsByStation(int stationId)
        {
            return new ObservableCollection<Sensor>(
                _dbContext.Sensors
                .Include(s => s.assignedStation)
                .Where(s => s.assignedStation != null && s.assignedStation.Id == stationId)
                .ToList()
            );
        }


        public void AddSensor(Sensor sensor)
        {
            _dbContext.Add(sensor);
            _dbContext.SaveChanges();
        }
        public void DeleteSensor(Sensor sensor)
        {
            _dbContext.Remove(sensor);
            _dbContext.SaveChanges();
        }

        public void UpdateSensor(Sensor sensor)
        {
            _dbContext.Sensors.Update(sensor);
            _dbContext.SaveChanges();
        }

        public void UpdateSensorStatus(Sensor sensor)
        {
            sensor.SensorStatus = !sensor.SensorStatus;
            _dbContext.Sensors.Update(sensor);
            _dbContext.SaveChanges();
        }

        public void UpdateOperationalStatus(Sensor sensor)
        {
            sensor.Operational = !sensor.Operational;
            _dbContext.Sensors.Update(sensor);
            _dbContext.SaveChanges();
        }

        public void UpdateDeliveryStatus(Sensor sensor)
        {
            sensor.Delivered = true;
            _dbContext.Sensors.Update(sensor);
            _dbContext.SaveChanges();
        }


    }
}
