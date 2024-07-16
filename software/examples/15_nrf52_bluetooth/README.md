# nRF52 with Bluetooth Example

After getting the accelerometer working in example 14, moving on to getting bluetooth working with the nrf52 Sense.

## Work

* Pulling in windows client from example 7
* Pulling in nrf52 code from resources below
* As always, run and upload with `pio run --target upload`
* For the csharp client, use `dotnet build` and `dotnet run` to run it once the nrf52 is running.

## Resources

* [Seeed Wiki](https://wiki.seeedstudio.com/XIAO-BLE-Sense-Bluetooth-Usage/) says to use this library
  * https://github.com/arduino-libraries/ArduinoBLE
  * But also read this library doesn't work for the nrf52.
* Other links
  * https://forum.seeedstudio.com/t/xiao-seeed-nrf52840-sense-sending-accelerometer-data-through-ble-not-working/275391/2
  * https://forum.arduino.cc/t/arduinoble-notify-issue-with-blecharacteristic/1062011/5