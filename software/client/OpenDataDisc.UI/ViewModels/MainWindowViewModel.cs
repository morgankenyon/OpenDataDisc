﻿using InTheHand.Bluetooth;
using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.Extensions;
using OpenDataDisc.UI.Models;
using OpenDataDisc.UI.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenDataDisc.UI.ViewModels;

public enum MainWindowState
{
    Unconnected = 0,
    Connecting = 1,
    Connected = 2,
    Disconnecting = 3
}

public class MainWindowViewModel : ViewModelBase
{
    //di references
    private readonly ISensorService _sensorService;

    //ui accessed variables
    private SelectedDeviceViewModel? _selectedDevice;
    public SelectedDeviceViewModel? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    private MainWindowState _currentState;

    public MainWindowState CurrentState
    {
        get => _currentState;
        set => this.RaiseAndSetIfChanged(ref _currentState, value);
    }

    private int _messageCount;
    public int MessageCount
    {
        get => _messageCount;
        set => this.RaiseAndSetIfChanged(ref _messageCount, value);
    }

    private double _messageRate;
    public double MessageRate
    {
        get => _messageRate;
        set => this.RaiseAndSetIfChanged(ref _messageRate, value);
    }

    public string CountText => $"Messages received: {_messageCount}";
    public string MessageRateText => $"Message Rate: {_messageRate} msg/sec";
    public string ChannelCountText => $"Channel Count: {SensorChannel.Reader.Count}";

    //interaction to launch bluetooth selector window
    public Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?> ShowBluetoothDialog { get; }
    public Interaction<ConfirmationWindowViewModel, ConfirmationResult> ShowConfirmationDialog { get; }

    //command for propagating selected device
    public ICommand SelectBluetoothDeviceCommand { get; }

    //extra    
    private static Channel<SensorData> SensorChannel = Channel.CreateUnbounded<SensorData>();
    public ObservableCollection<string> Messages { get; } = new();
    private CancellationTokenSource? _deviceConnectedTokenSource;
    private CancellationTokenSource? _messageRateTokenSource;
    
    public MainWindowViewModel(ISensorService sensorService)
    {
        CurrentState = MainWindowState.Unconnected;

        ShowBluetoothDialog = new Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?>();
        ShowConfirmationDialog = new Interaction<ConfirmationWindowViewModel, ConfirmationResult>();

        SelectBluetoothDeviceCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (CurrentState == MainWindowState.Connected)
            {
                var confirmationDialog = new ConfirmationWindow();

                var message = new ConfirmationWindowViewModel("Would you like to disconnect your bluetooth device?");
                var result = await ShowConfirmationDialog.Handle(message);
                if (result == ConfirmationResult.Yes)
                {
                    _deviceConnectedTokenSource?.Cancel();
                }

            }
            else
            {
                _deviceConnectedTokenSource?.Cancel();
                var bluetooth = new BluetoothSelectorViewModel();

                var result = await ShowBluetoothDialog.Handle(bluetooth);

                if (result != null)
                {
                    _deviceConnectedTokenSource = new CancellationTokenSource();
                    ListenToDevice(result, _deviceConnectedTokenSource.Token);
                }
            }
        });

        _sensorService = sensorService;

        this.WhenAnyValue(x => x.MessageCount)
            .Subscribe(_ => {
                this.RaisePropertyChanged(nameof(CountText));
                this.RaisePropertyChanged(nameof(ChannelCountText));
            });

        this.WhenAnyValue(x => x.MessageRate)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(MessageRateText)));

        this.WhenAnyValue(x => x.CurrentState)
            .Subscribe(_ => ControlMessageRateCalculation());
    }

    private void ControlMessageRateCalculation()
    {
        switch (CurrentState)
        {
            case MainWindowState.Connected:
                _messageRateTokenSource = new CancellationTokenSource();
                //intentionally not await, letting token stop it
                CalculateMessagesPerSecond(_messageRateTokenSource.Token);
                break;
            case MainWindowState.Unconnected:
            case MainWindowState.Disconnecting:
            case MainWindowState.Connecting:
                _messageRateTokenSource?.Cancel();
                break;
        }
    }

    private async void CalculateMessagesPerSecond(CancellationToken token)
    {
        try
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await periodicTimer.WaitForNextTickAsync(token))
            {
                var messages = await _sensorService.MessagesReceivedInLastNSeconds(15, token);
                var rate = Math.Round(messages / 15.0, 1);
                MessageRate = rate;
            }
        }
        catch (OperationCanceledException oce)
        {
            //TODO: probably log something, but don't want to rethrow here
        }
    }

    /// <summary>
    /// This consumes anything coming from the sensor channel and saves 
    /// it to the db
    /// </summary>
    /// <returns></returns>
    private async Task WriteToDatabase(CancellationToken token)
    {
        try
        {
            await foreach(var sensorData in SensorChannel.Reader.ReadAllAsync(token))
            {
                await _sensorService.SaveSensorData(sensorData, token);
            }
        }
        catch (OperationCanceledException oce)
        {
            //TODO: probably log something but don't want to throw in this case
        }
    }

    static EventHandler<GattCharacteristicValueChangedEventArgs> BuildNotifyEventHandler(
        ObservableCollection<string> messages,
        Action updateCount)
    {
        EventHandler<GattCharacteristicValueChangedEventArgs> privateMethod = (object? sender, GattCharacteristicValueChangedEventArgs e) =>
        {
            if (e.Value != null)
            {
                var strR = System.Text.Encoding.Default.GetString(e.Value);
                var strReceived = strR.Split("\0")[0];

                if (!string.IsNullOrWhiteSpace(strReceived))
                {
                    //messages.Add(strReceived);
                    updateCount();
                    SensorChannel.Writer.TryWrite(new SensorData(strReceived));
                }
                else
                {
                    Console.WriteLine($"Received - {e.Value} - {strReceived}");
                }
            }
            else if (e.Error != null)
            {
                Console.WriteLine($"Received Error - {e.Error.Message}");
            }
            else
            {
                Console.WriteLine("Received message");
            }
        };

        return privateMethod;
    }

    static EventHandler BuildDisconnectEventHandler(
        CancellationTokenSource? cancellationTokenSource)
    {
        EventHandler privateMethod = (object? sender, System.EventArgs e) =>
        {
            cancellationTokenSource?.Cancel();
        };

        return privateMethod;
    }

    private void UpdateCount()
    {
        this.MessageCount++;
    }

    private async void ListenToDevice(SelectedDeviceViewModel selectedDevice, CancellationToken token)
    {
        CurrentState = MainWindowState.Connecting;
        var device = selectedDevice.Device;

        var writeToDatabaseTask = WriteToDatabase(token).ConfigureAwait(false);

        if (device != null)
        {
            //name of device is changing after connecting for some reason
            await device.Gatt.ConnectAsync();

            //cancel token if device disconnects
            device.GattServerDisconnected += BuildDisconnectEventHandler(_deviceConnectedTokenSource);

            var service = await device.Gatt.GetPrimaryServiceAsync(Constants.ServiceUuid);

            if (service != null)
            {
                var chars = await service.GetCharacteristicAsync(Constants.CharacteristicUuid);
                if (chars != null)
                {
                    chars.CharacteristicValueChanged += BuildNotifyEventHandler(Messages, UpdateCount);
                    await chars.StartNotificationsAsync();

                    SelectedDevice = selectedDevice;
                    CurrentState = MainWindowState.Connected;

                    await Task.Delay(Timeout.Infinite, token)
                        .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                    await chars.SafeStopNotificationsAsync();
                }
            }

            CurrentState = MainWindowState.Disconnecting;

            device.Gatt.SafeDisconnect();

            await writeToDatabaseTask;
            SelectedDevice = null;
        }

        CurrentState = MainWindowState.Unconnected;
    }
}