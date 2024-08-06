using InTheHand.Bluetooth;

//Following this example: https://github.com/inthehand/32feet/blob/main/Samples/BluetoothClientApp/BluetoothClientApp/MainPage.xaml.cs

Console.WriteLine("Starting up the bluetooth!");

var serviceUuid = BluetoothUuid.FromGuid(Guid.Parse("900e9509-a0b2-4d89-9bb6-b5e011e758b0"));
var characteristicUuid = BluetoothUuid.FromGuid(Guid.Parse("6ef4cd45-7223-43b2-b5c9-d13410b494f5"));
RequestDeviceOptions options = new RequestDeviceOptions();
var scanFilter = new BluetoothLEScanFilter
{
    Name = "NRF52"
};
scanFilter.Services.Add(serviceUuid);
options.Filters.Add(scanFilter);

static void NotifyEventHandler(object? sender, GattCharacteristicValueChangedEventArgs e)
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
            Console.WriteLine($"Received - {str}");
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
}

BluetoothDevice device = await Bluetooth.RequestDeviceAsync(options);
if (device != null)
{
    Console.WriteLine($"Device {device.Name} found");
    if (!device.IsPaired)
    {
        Console.WriteLine($"Connecting to device {device.Name}");
        await device.Gatt.ConnectAsync();

        Console.WriteLine("Connected to device");
        Console.WriteLine("Connecting to service");
        var service = await device.Gatt.GetPrimaryServiceAsync(serviceUuid);
        if (service != null)
        {
            Console.WriteLine("Connected to service");
            Console.WriteLine("Connecting to characteristic");
            var characteristic = await service.GetCharacteristicAsync(characteristicUuid);
            if (characteristic != null)
            {
                Console.WriteLine("Connected to characteristic");
                var characterDescriptor = characteristic.UserDescription;
                Console.WriteLine($"characteristic descriptor: {characterDescriptor}");
                characteristic.CharacteristicValueChanged += NotifyEventHandler;
                await characteristic.StartNotificationsAsync();

                Console.WriteLine("Waiting for messages");
                await Task.Delay(10000); //delay for 10 seconds

                Console.WriteLine("Stopping messages");
                await characteristic.StopNotificationsAsync();
            }
        }
    }

    //disconnect with done
    device.Gatt.Disconnect();
}
Console.WriteLine("Wrapping up project");