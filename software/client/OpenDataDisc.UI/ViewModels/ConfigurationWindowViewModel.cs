using InTheHand.Bluetooth;
using OpenDataDisc.UI.Extensions;
using ReactiveUI;
using System;

namespace OpenDataDisc.UI.ViewModels
{
    public class ConfigurationWindowViewModel : ViewModelBase
    {
        public ConfigurationWindowViewModel() { }

        private int _messageCount;
        public int MessageCount
        {
            get => _messageCount;
            set => this.RaiseAndSetIfChanged(ref _messageCount, value);
        }

        private string _dataString = string.Empty;
        public string DataString
        {
            get => _dataString;
            set => this.RaiseAndSetIfChanged(ref _dataString, value);
        }

        private ConfigurationStep _step;
        public ConfigurationStep Step
        {
            get => _step;
            set => this.RaiseAndSetIfChanged(ref _step, value);
        }

        public void HandleMessage(object? sender, GattCharacteristicValueChangedEventArgs e)
        {
            var (data, errorMessage) = e.ExtractSensorData();

            if (data != null)
            {
                MessageCount++;
                DataString = data.ToString() ?? "";
            }
            else
            {
                //TODO: log this somewhere more useful
                Console.WriteLine(errorMessage);
            }
        }
    }

    public enum ConfigurationStep
    {
        Start,
        AccXSetup,
        AccXRecording,
        AccYSetup,
        AccYRecording,
        AccZSetup,
        AccZRecording,
        GyroSetup,
        GyroRecording,
    }
}
