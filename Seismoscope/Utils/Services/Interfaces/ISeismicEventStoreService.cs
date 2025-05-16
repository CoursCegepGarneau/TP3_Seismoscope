using Seismoscope.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Utils.Services.Interfaces
{
    public interface ISeismicEventStoreService
    {
        void AddEvent(SeismicEvent seismicEvent);
        IReadOnlyCollection<SeismicEvent> GetLastSeismicEvents();
    }
}
