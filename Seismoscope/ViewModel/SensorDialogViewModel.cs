using System;
using System.Windows;
using System.Windows.Input;
using Seismoscope.Utils;
using Seismoscope.Utils.Commands;

namespace Seismoscope.ViewModel
{
    public class SensorDialogViewModel : BaseViewModel
    {
        // Propriétés pour les champs
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

        // Booléens pour contrôler l'affichage des champs
        public bool ShowName { get; set; }
        public bool ShowFrequency { get; set; }
        public bool ShowTreshold { get; set; }

        // Commandes
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private Visibility _errorVisibility = Visibility.Collapsed;
        public Visibility ErrorVisibility
        {
            get => _errorVisibility;
            set
            {
                _errorVisibility = value;
                OnPropertyChanged(nameof(ErrorVisibility));
            }
        }

        // Action appelée par la vue pour fermer la fenêtre
        public Action<bool> CloseAction { get; set; }

        public SensorDialogViewModel()
        {
            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnConfirm()
        {
            // Validation pour la fréquence si elle doit être affichée
            if (ShowFrequency)
            {
                if (!double.TryParse(Frequency, out double newFreq))
                {
                    ErrorMessage="Veuillez entrer une valeur numérique valide pour la fréquence.";
                    ErrorVisibility = Visibility.Visible;
                    return;
                }
                else if (newFreq < 1 || newFreq > 100)
                {
                    ErrorMessage="La fréquence doit être comprise entre 1 et 100.";
                    ErrorVisibility = Visibility.Visible;
                    return;
                }
                else
                {
                    ErrorVisibility = Visibility.Collapsed;
                }
                
            }

            // Validation pour le seuil si nécessaire
            if (ShowTreshold)
            {
                if (!double.TryParse(Treshold, out double newThr))
                {
                    ErrorMessage = "Entrez une valeur numérique valide pour le seuil.";
                    ErrorVisibility = Visibility.Visible;
                    return;
                }
                else if (newThr < 0.1 || newThr > 10)
                {
                    ErrorMessage = "Le seuil doit être compris entre 0,1mm et 10mm.";
                    ErrorVisibility = Visibility.Visible;
                    return;
                }
                else
                {
                    ErrorVisibility = Visibility.Collapsed;
                }
            }

            // Validation pour le nom si affiché
            if (ShowName)
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ErrorMessage = "Le nom ne peut être vide.";
                    ErrorVisibility = Visibility.Visible;
                    return;
                }
                else
                {
                    ErrorVisibility = Visibility.Collapsed;
                }
            }

            // Toutes les validations sont passées : on ferme le dialogue en indiquant le succès.
            CloseAction?.Invoke(true);
        }

        private void OnCancel()
        {
            CloseAction?.Invoke(false);
        }
    }
}
