using Moq;
using Seismoscope.Utils.Commands;
using Seismoscope.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace SeismoscopeTest.ViewModel
{
    public class SensorDialogViewModelTests
    {
        private readonly SensorDialogViewModel _viewModel;
        private readonly Mock<Action<bool>> _mockCloseAction;

        public SensorDialogViewModelTests()
        {
            _mockCloseAction = new Mock<Action<bool>>();

            _viewModel = new SensorDialogViewModel()
            {
                CloseAction = _mockCloseAction.Object
            };
        }

        [Fact]
        public void ConfirmCommand_ShouldCloseDialog_WhenValidInputs()
        {
            // Arrange
            _viewModel.Name = "Sensor 1";
            _viewModel.Frequency = "50";
            _viewModel.Treshold = "5";
            _viewModel.ShowName = true;
            _viewModel.ShowFrequency = true;
            _viewModel.ShowTreshold = true;

            // Act
            _viewModel.ConfirmCommand.Execute(null);

            // Assert
            _mockCloseAction.Verify(c => c.Invoke(true), Times.Once);
        }

        [Fact]
        public void ConfirmCommand_ShouldNotCloseDialog_WhenInvalidFrequency()
        {
            // Arrange
            _viewModel.Name = "Sensor 1";
            _viewModel.Frequency = "150";
            _viewModel.Treshold = "5";
            _viewModel.ShowName = true;
            _viewModel.ShowFrequency = true;
            _viewModel.ShowTreshold = true;

            // Act
            _viewModel.ConfirmCommand.Execute(null);

            // Assert
            _mockCloseAction.Verify(c => c.Invoke(It.IsAny<bool>()), Times.Never);
        }


        [Fact]
        public void ConfirmCommand_ShouldNotCloseDialog_WhenInvalidTreshold()
        {
            // Arrange
            _viewModel.Name = "Sensor 1";
            _viewModel.Frequency = "50";
            _viewModel.Treshold = "15";
            _viewModel.ShowName = true;
            _viewModel.ShowFrequency = true;
            _viewModel.ShowTreshold = true;

            // Act
            _viewModel.ConfirmCommand.Execute(null);

            // Assert
            _mockCloseAction.Verify(c => c.Invoke(It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void ConfirmCommand_ShouldNotCloseDialog_WhenInvalidName()
        {
            // Arrange
            _viewModel.Name = "";
            _viewModel.Frequency = "50";
            _viewModel.Treshold = "5";
            _viewModel.ShowName = true;
            _viewModel.ShowFrequency = true;
            _viewModel.ShowTreshold = true;

            // Act
            _viewModel.ConfirmCommand.Execute(null);

            // Assert
            _mockCloseAction.Verify(c => c.Invoke(It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void CancelCommand_ShouldInvokeCloseActionWithFalse()
        {
            // Act
            _viewModel.CancelCommand.Execute(null);

            // Assert: CloseAction should be invoked with 'false' on cancel
            _mockCloseAction.Verify(c => c.Invoke(false), Times.Once);
        }
    }
}
