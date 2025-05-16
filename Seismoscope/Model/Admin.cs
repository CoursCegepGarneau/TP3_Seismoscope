using Seismoscope.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seismoscope.Model
{
    public class Admin : User
    {
        public override UserRole Role => UserRole.Admin;
    }
}
