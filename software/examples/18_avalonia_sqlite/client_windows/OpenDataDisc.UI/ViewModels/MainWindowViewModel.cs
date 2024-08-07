using InTheHand.Bluetooth;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SQLite;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenDataDisc.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly BluetoothUuid serviceUuid = BluetoothUuid.FromGuid(Guid.Parse("900e9509-a0b2-4d89-9bb6-b5e011e758b0"));
    private readonly BluetoothUuid characteristicUuid = BluetoothUuid.FromGuid(Guid.Parse("6ef4cd45-7223-43b2-b5c9-d13410b494f5"));
    public ICommand SelectBluetoothDeviceCommand { get; }
    public ICommand CreateDatabaseCommand { get; }
    public Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?> ShowBluetoothDialog { get; }

    private SelectedDeviceViewModel? _selectedDevice;
    public SelectedDeviceViewModel? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    public ObservableCollection<string> Messages { get; } = new();

    private CancellationTokenSource? _cancellationTokenSource;
    public MainWindowViewModel()
    {
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

        CreateDatabaseCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            SQLiteConnection sqlConn;

            sqlConn = new SQLiteConnection("Data Source=opendatadisc.db;Version=3;New=True;Compress=True");

            try
            {
                sqlConn.Open();
            }
            catch (Exception e)
            {

            }

            //create table
            SQLiteCommand command;
            command = sqlConn.CreateCommand();

            //create table, checking to see if it already exists or not
            string createTable = "CREATE TABLE Throws (Name VARCHAR(20), Number INT)";
            string tableCheckSql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='Throws';";
            command.CommandText = tableCheckSql;
            DbDataReader tableCheckReader = await command.ExecuteReaderAsync();

            if (!tableCheckReader.HasRows)
            {
                command.CommandText = createTable;
                await command.ExecuteNonQueryAsync();
            }

            //insert dummy data
            Random rnd = new Random();
            int number = rnd.Next(1, 10000);

            command = sqlConn.CreateCommand(); //having an issue after doing the table check above, not sure why
            string insertStatement = $"INSERT INTO Throws (Name, Number) VALUES ('Bob_{number}', {number});";
            command.CommandText = insertStatement;
            await command.ExecuteNonQueryAsync();

            //read data
            string selectStatement = "SELECT * FROM Throws";
            command.CommandText = selectStatement;

            DbDataReader reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                Messages.Add(name);
            }
        });
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
        var device = selectedDevice.Device;

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

                    await Task.Delay(Timeout.Infinite, token)
                        .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

                    await chars.StopNotificationsAsync();
                }
            }

            device.Gatt.Disconnect();
        }
    }
}