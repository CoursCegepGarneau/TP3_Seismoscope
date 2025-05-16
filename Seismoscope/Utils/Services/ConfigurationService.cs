using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Seismoscope.Utils.Services.Interfaces;

namespace Seismoscope.Utils.Services
{
    public class ConfigurationService: IConfigurationService
    {
        public string? GetDbPath() 
        {
            return ConfigurationManager.AppSettings["DbPath"];
        }
    }
}
