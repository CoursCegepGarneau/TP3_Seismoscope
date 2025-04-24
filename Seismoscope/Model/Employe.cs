using Seismoscope.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Model
{
    public class Employe : User
    {
        public override UserRole Role => UserRole.Employe;

        public int StationId { get; set; }
        public Station Station { get; set; }
    }
}
