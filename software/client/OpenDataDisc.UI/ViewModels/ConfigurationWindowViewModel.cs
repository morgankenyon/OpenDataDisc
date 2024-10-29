using InTheHand.Bluetooth;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.Extensions;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDataDisc.UI.ViewModels
{
    public class ConfigurationWindowViewModel : ViewModelBase
    {
        private int measurementsCount = 200;
        private List<double> accMeasurements = new List<double>();
        private List<(double, double, double)> gyroMeasurements = new List<(double, double, double)>();
        public ConfigurationWindowViewModel()
        {
            this.WhenAnyValue(x => x.Step)
                .Subscribe(_ => {
                    this.RaisePropertyChanged(nameof(StepText));
                    this.RaisePropertyChanged(nameof(ConfigurationInstructionsText));
                    this.RaisePropertyChanged(nameof(ShowNext));
                    this.RaisePropertyChanged(nameof(ShowClose));
                    this.RaisePropertyChanged(nameof(IsNextButtonEnabled));
                });
            this.WhenAnyValue(x => x.ConfiguringValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(ConfiguringValueText)));
            this.WhenAnyValue(x => x.AccXValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SensorValueText)));
            this.WhenAnyValue(x => x.AccYValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SensorValueText)));
            this.WhenAnyValue(x => x.AccZValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SensorValueText)));
            this.WhenAnyValue(x => x.GyroXValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SensorValueText)));
            this.WhenAnyValue(x => x.GyroYValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SensorValueText)));
            this.WhenAnyValue(x => x.GyroZValue)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SensorValueText)));
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

        private double _accXValue;
        public double AccXValue
        {
            get => _accXValue;
            set => this.RaiseAndSetIfChanged(ref _accXValue, value);
        }

        private double _accYValue;
        public double AccYValue
        {
            get => _accYValue;
            set => this.RaiseAndSetIfChanged(ref _accYValue, value);
        }

        private double _accZValue;
        public double AccZValue
        {
            get => _accZValue;
            set => this.RaiseAndSetIfChanged(ref _accZValue, value);
        }

        private double _gyroXValue;
        public double GyroXValue
        {
            get => _gyroXValue;
            set => this.RaiseAndSetIfChanged(ref _gyroXValue, value);
        }

        private double _gyroYValue;
        public double GyroYValue
        {
            get => _gyroYValue;
            set => this.RaiseAndSetIfChanged(ref _gyroYValue, value);
        }

        private double _gyroZValue;
        public double GyroZValue
        {
            get => _gyroZValue;
            set => this.RaiseAndSetIfChanged(ref _gyroZValue, value);
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
        public string SensorValueText => $"{Math.Round(AccXValue, 3)}, {Math.Round(AccYValue, 3)}, {Math.Round(AccZValue, 3)}, {Math.Round(GyroXValue, 3)}, {Math.Round(GyroYValue, 3)}, {Math.Round(GyroZValue, 3)}";

        public bool ShowNext => Step != ConfigurationStep.Finished;
        public bool IsNextButtonEnabled => Step == ConfigurationStep.Start
            || Step == ConfigurationStep.AccXSetup
            || Step == ConfigurationStep.AccYSetup
            || Step == ConfigurationStep.AccZSetup
            || Step == ConfigurationStep.GyroSetup;
        public bool ShowClose => Step == ConfigurationStep.Finished;

        public DiscConfigurationData? DiscConfiguration;

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
                    ConfiguringValue = data.AccX;
                    break;
                case ConfigurationStep.AccXRecording:
                    ConfiguringValue = data.AccX;
                    accMeasurements.Add(ConfiguringValue.Value);
                    MoveToNextStepIfEnoughMeasurements();
                    break;
                case ConfigurationStep.AccYSetup:
                    ConfiguringValue = data.AccY;
                    break;
                case ConfigurationStep.AccYRecording:
                    ConfiguringValue = data.AccY;
                    accMeasurements.Add(ConfiguringValue.Value);
                    MoveToNextStepIfEnoughMeasurements();
                    break;
                case ConfigurationStep.AccZSetup:
                    ConfiguringValue = data.AccZ;
                    break;
                case ConfigurationStep.AccZRecording:
                    ConfiguringValue = data.AccZ;
                    accMeasurements.Add(ConfiguringValue.Value);
                    MoveToNextStepIfEnoughMeasurements();
                    break;
                case ConfigurationStep.GyroRecording:
                    gyroMeasurements.Add((data.GyroX, data.GyroY, data.GyroZ));
                    MoveToNextStepIfEnoughMeasurements();
                    break;
                default:
                    ConfiguringValue = null;
                    break;
            }
        }

        private void MoveToNextStepIfEnoughMeasurements()
        {
            if (accMeasurements.Count > measurementsCount || gyroMeasurements.Count > measurementsCount)
            {
                AdvanceToNextStep();
                TakeConfigurationAverage();
            }
        }

        private void TakeConfigurationAverage()
        {
            switch (Step)
            {
                case ConfigurationStep.AccYSetup:
                    AccXValue = accMeasurements.DefaultIfEmpty(0).Average(x => x);
                    accMeasurements.Clear();
                    break;
                case ConfigurationStep.AccZSetup:
                    AccYValue = accMeasurements.DefaultIfEmpty(0).Average(x => x);
                    accMeasurements.Clear();
                    break;
                case ConfigurationStep.GyroSetup:
                    AccZValue = accMeasurements.DefaultIfEmpty(0).Average(x => x);
                    accMeasurements.Clear();
                    break;
                case ConfigurationStep.Finished:
                    //TODO: maybe move this into one linq statement
                    GyroXValue = gyroMeasurements.Average(x => x.Item1);
                    GyroYValue = gyroMeasurements.Average(x => x.Item2);
                    GyroZValue = gyroMeasurements.Average(x => x.Item3);
                    gyroMeasurements.Clear();
                    DiscConfiguration = new DiscConfigurationData(
                        "rst",
                        DateTime.UtcNow.Ticks,
                        AccXValue,
                        AccYValue,
                        AccZValue,
                        GyroXValue,
                        GyroYValue,
                        GyroZValue);
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
