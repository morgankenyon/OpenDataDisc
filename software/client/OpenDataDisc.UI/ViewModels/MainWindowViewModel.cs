using InTheHand.Bluetooth;
using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.Models;
using OpenDataDisc.UI.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
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
    private readonly BluetoothUuid serviceUuid = BluetoothUuid.FromGuid(Guid.Parse("900e9509-a0b2-4d89-9bb6-b5e011e758a0"));
    private readonly BluetoothUuid characteristicUuid = BluetoothUuid.FromGuid(Guid.Parse("6ef4cd45-7223-43b2-b5c9-d13410b494a5"));

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
    private CancellationTokenSource? _cancellationTokenSource;
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

                try
                {
                    var message = new ConfirmationWindowViewModel("Would you like to disconnect your bluetooth device?");
                    var result = await ShowConfirmationDialog.Handle(message);
                    if (result == ConfirmationResult.Yes)
                    {
                        _cancellationTokenSource?.Cancel();
                    }
                }
                catch (Exception ex)
                {
                    MessageCount++;
                }

            }
            else
            {
                _cancellationTokenSource?.Cancel();
                var bluetooth = new BluetoothSelectorViewModel();

                var result = await ShowBluetoothDialog.Handle(bluetooth);

                if (result != null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    ListenToDevice(result, _cancellationTokenSource.Token);
                }
                MessageCount++;
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
        var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await periodicTimer.WaitForNextTickAsync(token))
        {
            var messages = await _sensorService.MessagesReceivedInLastNSeconds(15, token);
            var rate = Math.Round(messages / 15.0, 1);
            MessageRate = rate;
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

            var service = await device.Gatt.GetPrimaryServiceAsync(serviceUuid);

            if (service != null)
            {
                var chars = await service.GetCharacteristicAsync(characteristicUuid);
                if (chars != null)
                {
                    chars.CharacteristicValueChanged += BuildNotifyEventHandler(Messages, UpdateCount);
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
            SelectedDevice = null;
        }

        CurrentState = MainWindowState.Unconnected;
    }
}