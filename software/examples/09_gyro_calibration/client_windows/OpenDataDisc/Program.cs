using System.Text.Json;
using System.Text.RegularExpressions;
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

string gyroSensorPattern = @"Rot: *(-?[\d.]+), *(-?[\d.]+), *(-?[\d.]+)";
Regex gyroRegex = new Regex(gyroSensorPattern);
List<string> GyroReadings = new List<string>();
void NotifyEventHandler(object? sender, GattCharacteristicValueChangedEventArgs e)
{
    if (e.Value != null)
    {
        var strReceived = System.Text.Encoding.Default.GetString(e.Value);
        if (gyroRegex.IsMatch(strReceived))
        {
            GyroReadings.Add(strReceived);
        }
        else
        {
            Console.WriteLine($"Received none matching string - '{strReceived}'");
        }
    }
    else if (e.Error != null)
    {
        Console.WriteLine($"Received Error - {e.Error.Message}");
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

                
                Console.WriteLine("Pulling calibration data");
                await Task.Delay(10000); //delay for 60 seconds

                Console.WriteLine("Stopping messages");
                await characteristic.StopNotificationsAsync();
            }
        }
    }

    //disconnect with done
    device.Gatt.Disconnect();
}

Console.WriteLine($"We read: {GyroReadings.Count} readings");
Console.WriteLine($"First line is: {GyroReadings.First()}");

double calibrationX = 0.0;
double calibrationY = 0.0;
double calibrationZ = 0.0;
foreach (var reading in GyroReadings)
{
    MatchCollection mc = gyroRegex.Matches(reading);
    var firstGroups = mc.First().Groups;
    
    if (firstGroups != null && firstGroups.Count == 4)
    {
        var firstValue = firstGroups[1];
        calibrationX += Convert.ToDouble(firstValue.ToString());
        var secondValue = firstGroups[2];
        calibrationY += Convert.ToDouble(secondValue.ToString());
        var thirdValue = firstGroups[3];
        calibrationZ += Convert.ToDouble(thirdValue.ToString());
    }
}

calibrationX = calibrationX / GyroReadings.Count;
calibrationY = calibrationY / GyroReadings.Count;
calibrationZ = calibrationZ / GyroReadings.Count;

Console.WriteLine($"calibrationX: {Math.Round(calibrationX, 2)}");
Console.WriteLine($"calibrationY: {Math.Round(calibrationY, 2)}");
Console.WriteLine($"calibrationZ: {Math.Round(calibrationZ, 2)}");

var gyroData = new {
    x = Math.Round(calibrationX, 2),
    y = Math.Round(calibrationY, 2),
    z = Math.Round(calibrationZ, 2)
};

string json = JsonSerializer.Serialize(gyroData);
Console.WriteLine(json);
File.WriteAllText("../gyro_calibration.json", json);