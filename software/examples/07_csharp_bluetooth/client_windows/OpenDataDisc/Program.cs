using InTheHand.Bluetooth;

//Following this example: https://github.com/inthehand/32feet/blob/main/Samples/BluetoothClientApp/BluetoothClientApp/MainPage.xaml.cs

Console.WriteLine("Starting up the bluetooth!");

var serviceUuid = BluetoothUuid.FromGuid(Guid.Parse("91bad492-b950-4226-aa2b-4ede9fa42f59"));
var characteristicUuid = BluetoothUuid.FromGuid(Guid.Parse("cba1d466-344c-4be3-ab3f-189f80dd7518"));
RequestDeviceOptions options = new RequestDeviceOptions();
var scanFilter = new BluetoothLEScanFilter
{
    Name = "ESP32_Server"
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