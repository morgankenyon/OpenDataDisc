﻿using InTheHand.Bluetooth;
using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
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
    //reference values
    private readonly BluetoothUuid serviceUuid = BluetoothUuid.FromGuid(Guid.Parse("900e9509-a0b2-4d89-9bb6-b5e011e758b0"));
    private readonly BluetoothUuid characteristicUuid = BluetoothUuid.FromGuid(Guid.Parse("6ef4cd45-7223-43b2-b5c9-d13410b494f5"));

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

    //interaction to launch bluetooth selector window
    public Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?> ShowBluetoothDialog { get; }
    
    //command for propagating selected device
    public ICommand SelectBluetoothDeviceCommand { get; }

    //extra    
    private static Channel<SensorData> SensorChannel = Channel.CreateUnbounded<SensorData>();
    public ObservableCollection<string> Messages { get; } = new();
    private CancellationTokenSource? _cancellationTokenSource;
    
    public MainWindowViewModel(ISensorService sensorService)
    {
        CurrentState = MainWindowState.Unconnected;

        ShowBluetoothDialog = new Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?>();

        SelectBluetoothDeviceCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _cancellationTokenSource?.Cancel();
            var bluetooth = new BluetoothSelectorViewModel();

            var result = await ShowBluetoothDialog.Handle(bluetooth);

            if (result != null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await ListenToDevice(result, _cancellationTokenSource.Token);
            }
        });

        _sensorService = sensorService;
    }

    /// <summary>
    /// This consumes anything coming from the sensor channel and saves 
    /// it to the db
    /// </summary>
    /// <returns></returns>
    private async Task WriteToDatabase()
    {
        await foreach(var sensorData in SensorChannel.Reader.ReadAllAsync())
        {
            await _sensorService.SaveSensorData(sensorData);
        }
    }

    static EventHandler<GattCharacteristicValueChangedEventArgs> BuildNotifyEventHandler(
        ObservableCollection<string> messages)
    {
        EventHandler<GattCharacteristicValueChangedEventArgs> privateMethod = (object? sender, GattCharacteristicValueChangedEventArgs e) =>
        {
            if (e.Value != null)
            {
                var strReceived = System.Text.Encoding.Default.GetString(e.Value);
                if (double.TryParse(strReceived, out var result))
                {
                    Console.WriteLine($"Received - {result}");
                }
                else if (e.Value is System.Byte[])
                {
                    var bytes = e.Value as System.Byte[];
                    var str = System.Text.Encoding.Default.GetString(bytes);
                    messages.Add(str);
                    SensorChannel.Writer.TryWrite(new SensorData(str));
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

    private async Task ListenToDevice(SelectedDeviceViewModel selectedDevice, CancellationToken token)
    {
        CurrentState = MainWindowState.Connecting;
        var device = selectedDevice.Device;

        var writeToDatabaseTask = WriteToDatabase().ConfigureAwait(false);

        if (device != null)
        {
            //name of device is changing after connecting for some reason
            await device.Gatt.ConnectAsync();

            var service = await device.Gatt.GetPrimaryServiceAsync(serviceUuid);

            if (service != null)
            {
                var chars = await service.GetCharacteristicAsync(characteristicUuid);
                if (chars != null)
                {
                    chars.CharacteristicValueChanged += BuildNotifyEventHandler(Messages);
                    await chars.StartNotificationsAsync();

                    SelectedDevice = selectedDevice;
                    CurrentState = MainWindowState.Connected;

                    await Task.Delay(Timeout.Infinite, token)
                        .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                    await chars.StopNotificationsAsync();
                }
            }

            CurrentState = MainWindowState.Disconnecting;

            device.Gatt.Disconnect();

            await writeToDatabaseTask;
        }

        CurrentState = MainWindowState.Unconnected;
    }
}