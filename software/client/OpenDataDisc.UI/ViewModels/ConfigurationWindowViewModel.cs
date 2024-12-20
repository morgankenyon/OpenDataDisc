﻿using InTheHand.Bluetooth;
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
        private readonly string deviceId;
        private int configValueTrackingCount = 0;
        private int measurementCountThreshold = 500;
        private List<double> accMeasurements = new List<double>();
        private List<(double, double, double)> gyroMeasurements = new List<(double, double, double)>();

        public ConfigurationWindowViewModel(string deviceId)
        {
            this.deviceId = deviceId;

            this.WhenAnyValue(x => x.Step)
                .Subscribe(_ => {
                    this.RaisePropertyChanged(nameof(StepText));
                    this.RaisePropertyChanged(nameof(ConfigurationInstructionsText));
                    this.RaisePropertyChanged(nameof(ShowNext));
                    this.RaisePropertyChanged(nameof(ShowClose));
                    this.RaisePropertyChanged(nameof(IsNextButtonEnabled));
                    this.RaisePropertyChanged(nameof(ShouldShowConfiguringValue));
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

        private double _accXValue;
        private double _accYValue;
        private double _accZValue;
        private double _gyroXValue;
        private double _gyroYValue;
        private double _gyroZValue;

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

        public bool ShowNext => Step != ConfigurationStep.Finished;
        public bool IsNextButtonEnabled => Step == ConfigurationStep.Start
            || Step == ConfigurationStep.AccXSetup
            || Step == ConfigurationStep.AccYSetup
            || Step == ConfigurationStep.AccZSetup
            || Step == ConfigurationStep.GyroSetup;
        public bool ShouldShowConfiguringValue => Step == ConfigurationStep.AccXSetup
            || Step == ConfigurationStep.AccYSetup
            || Step == ConfigurationStep.AccZSetup;

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

        private void UpdateAccConfiguringValue(float data)
        {
            configValueTrackingCount++;
            if (configValueTrackingCount % 20 == 0)
            {
                ConfiguringValue = data;
            }
        }

        private void ConfirmConfiguringValue(SensorData data)
        {
            switch (Step)
            {
                case ConfigurationStep.AccXSetup:
                    UpdateAccConfiguringValue(data.AccX);
                    break;
                case ConfigurationStep.AccXRecording:
                    ConfiguringValue = data.AccX;
                    accMeasurements.Add(ConfiguringValue.Value);
                    MoveToNextStepIfEnoughMeasurements();
                    break;
                case ConfigurationStep.AccYSetup:
                    UpdateAccConfiguringValue(data.AccY);
                    break;
                case ConfigurationStep.AccYRecording:
                    ConfiguringValue = data.AccY;
                    accMeasurements.Add(ConfiguringValue.Value);
                    MoveToNextStepIfEnoughMeasurements();
                    break;
                case ConfigurationStep.AccZSetup:
                    UpdateAccConfiguringValue(data.AccZ);
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
            if (accMeasurements.Count > measurementCountThreshold || gyroMeasurements.Count > measurementCountThreshold)
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
                    _accXValue = accMeasurements.DefaultIfEmpty(0).Average(x => x);
                    accMeasurements.Clear();
                    break;
                case ConfigurationStep.AccZSetup:
                    _accYValue = accMeasurements.DefaultIfEmpty(0).Average(x => x);
                    accMeasurements.Clear();
                    break;
                case ConfigurationStep.GyroSetup:
                    _accZValue = accMeasurements.DefaultIfEmpty(0).Average(x => x);
                    accMeasurements.Clear();
                    break;
                case ConfigurationStep.Finished:
                    //TODO: maybe move this into one linq statement
                    _gyroXValue = gyroMeasurements.Average(x => x.Item1);
                    _gyroYValue = gyroMeasurements.Average(x => x.Item2);
                    _gyroZValue = gyroMeasurements.Average(x => x.Item3);
                    gyroMeasurements.Clear();
                    DiscConfiguration = new DiscConfigurationData(
                        deviceId,
                        DateTime.UtcNow.Ticks,
                        _accXValue,
                        _accYValue,
                        _accZValue,
                        _gyroXValue,
                        _gyroYValue,
                        _gyroZValue);
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
