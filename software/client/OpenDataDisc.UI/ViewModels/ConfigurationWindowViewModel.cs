using InTheHand.Bluetooth;
using OpenDataDisc.Services.Models;
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
                .Subscribe(_ => {
                    this.RaisePropertyChanged(nameof(StepText));
                    this.RaisePropertyChanged(nameof(ConfigurationInstructionsText));
                });
            this.WhenAnyValue(x => x.ConfiguringValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ConfiguringValueText)));
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

        private double? _configuringValue;
        public double? ConfiguringValue
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
        public string ConfiguringValueText => ConfiguringValue.HasValue ? $"{Math.Round(ConfiguringValue.Value, 3)}" : "";
        public string ConfigurationInstructionsText => Step switch
        {
            ConfigurationStep.Start => "Let's configure your OpenDataDisc. Please click Next when ready.",
            ConfigurationStep.AccXSetup => "Please position device along the X axis, this value below should display close to 1",
            ConfigurationStep.AccYSetup => "Please position device along the Y axis, this value below should display close to 1",
            ConfigurationStep.AccZSetup => "Please position device along the Z axis, this value below should display close to 1",
            ConfigurationStep.AccXRecording => "Recording, please don't touch device",
            ConfigurationStep.AccYRecording => "Recording, please don't touch device",
            ConfigurationStep.AccZRecording => "Recording, please don't touch device",
            ConfigurationStep.GyroSetup => "Please lay the device flat and don't touch it",
            ConfigurationStep.GyroRecording => "Recording, please don't touch device",
            ConfigurationStep.Finished => "All finished, you can click closed to start throwing!",
            _ => "Unknown, please reconnect your bluetooth device"
        };

        public void HandleMessage(object? sender, GattCharacteristicValueChangedEventArgs e)
        {
            var (data, errorMessage) = e.ExtractSensorData();

            if (data != null)
            {
                MessageCount++;
                DataString = data.ToString() ?? "";
                ConfirmConfiguringValue(data);
            }
            else
            {
                //TODO: log this somewhere more useful
                Console.WriteLine(errorMessage);
            }
        }

        private void ConfirmConfiguringValue(SensorData data)
        {
            switch (Step)
            {
                case ConfigurationStep.AccXSetup:
                case ConfigurationStep.AccXRecording:
                    ConfiguringValue = data.AccX;
                    break;
                case ConfigurationStep.AccYSetup:
                case ConfigurationStep.AccYRecording:
                    ConfiguringValue = data.AccY;
                    break;
                case ConfigurationStep.AccZSetup:
                case ConfigurationStep.AccZRecording:
                    ConfiguringValue = data.AccZ;
                    break;
                default:
                    ConfiguringValue = null;
                    break;
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
