using InTheHand.Bluetooth;
using OpenDataDisc.UI.Extensions;
using ReactiveUI;
using System;

namespace OpenDataDisc.UI.ViewModels
{
    public class ConfigurationWindowViewModel : ViewModelBase
    {
        public ConfigurationWindowViewModel()
        {
            this.WhenAnyValue(x => x.Step)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(StepText)));
        }

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

        private float _configuringValue;
        public float ConfiguringValue
        {
            get => _configuringValue;
            set => this.RaiseAndSetIfChanged(ref _configuringValue, value);
        }

        private ConfigurationStep _step = ConfigurationStep.Start;
        public ConfigurationStep Step
        {
            get => _step;
            set => this.RaiseAndSetIfChanged(ref _step, value);
        }

        public string StepText => $"Config Step: {_step}";

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

        public void AdvanceToNextStep()
        {
            Step = (ConfigurationStep)((int)Step + 1);
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
        Finished
    }
}
