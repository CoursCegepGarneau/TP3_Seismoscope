using System;
using System.Windows;
using System.Windows.Input;
using NLog;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;

namespace Seismoscope.ViewModel
{
    public class SensorDialogViewModel : BaseViewModel
    {
        readonly static Logger logger = LogManager.GetCurrentClassLogger();
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _frequency;
        public string Frequency
        {
            get => _frequency;
            set { _frequency = value; OnPropertyChanged(); }
        }

        private string _treshold;
        public string Treshold
        {
            get => _treshold;
            set { _treshold = value; OnPropertyChanged(); }
        }

        public bool ShowName { get; set; }
        public bool ShowFrequency { get; set; }
        public bool ShowTreshold { get; set; }
        public Action<bool> CloseAction { get; set; }


        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        private string _errorMessageName;
        public string ErrorMessageName
        {
            get => _errorMessageName;
            set
            {
                _errorMessageName = value;
                OnPropertyChanged(nameof(ErrorMessageName));
            }
        }
        private string _errorMessageFreq;
        public string ErrorMessageFreq
        {
            get => _errorMessageFreq;
            set
            {
                _errorMessageFreq = value;
                OnPropertyChanged(nameof(ErrorMessageFreq));
            }
        }
        private string _errorMessageTres;
        public string ErrorMessageTres
        {
            get => _errorMessageTres;
            set
            {
                _errorMessageTres = value;
                OnPropertyChanged(nameof(ErrorMessageTres));
            }
        }
        public SensorDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnConfirm()
        {
            if (ShowName)
            {
                if (Name.Empty())
                {
                    ErrorMessageName = "Le nom est requis";
                    return;
                }
                else
                    ErrorMessageName = "";
            }

            if (ShowFrequency)
            {
                if (!double.TryParse(Frequency, out double newFreq))
                {
                    ErrorMessageFreq = "Une valeur numérique doit être entrée";
                    return;
                }
                else if (newFreq < 1 || newFreq > 100)
                {
                    ErrorMessageFreq = "Valeur non valide";
                    return;
                }
                else
                    ErrorMessageFreq = "";
            }

            if (ShowTreshold)
            {
                if (!double.TryParse(Treshold, out double newThr))
                {
                    ErrorMessageTres = "La valeur doit être numérique";
                    return;
                }
                else if (newThr < 0.1 || newThr > 10)
                {
                    ErrorMessageTres = "Valeur non valide";
                    return;
                }
                else
                    ErrorMessageTres = "";

            }
            CloseAction?.Invoke(true);
        }

        private void OnCancel()
        {
            CloseAction?.Invoke(false);
        }
    }
}
