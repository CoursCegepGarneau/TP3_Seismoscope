 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Seismoscope.Model.Interfaces;
using Seismoscope.Utils;
using Seismoscope.Model;
using Seismoscope.Model.DAL;
using Seismoscope.Utils.Commands;
using Seismoscope.Utils.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Controls;

namespace Seismoscope.View
{
    /// <summary>
    /// Logique d'interaction pour SensorManagementView.xaml
    /// </summary>
    public partial class SensorManagementView : UserControl
    {
        public SensorManagementView()
        {
            InitializeComponent();
        }

    }
}
