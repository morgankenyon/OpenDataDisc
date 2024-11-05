# Random Notes

## Changes needed/Notes

* New Gyroscope
  * The gyroscope currently being used only supports up to 2,000 deg/s, which is ~333 RPM. Disc golf throws can go upwards of 1500 RPM. So that won't work long term.
  * I found [this gyroscope](https://www.mouser.com/ProductDetail/Analog-Devices/ADXRS649BBGZ?qs=WIvQP4zGanhEKWMUW9AK8A%3D%3D) that supports up to 20,000 deg/s, which is ~3,333 RPM, more than enough. It's just ~$120ish bucks.
* New Accelerometer
  * Also will need a new accelerometer to support up to 150 gs.
  * Might use [this one](https://www.mouser.com/ProductDetail/Analog-Devices/ADXL314WBCPZ-RL?qs=4ASt3YYao0VqiVnUWLAbOQ%3D%3D)
* G forces found in disc golf throws.
  * [Here's a link](https://www.reddit.com/r/discgolf/comments/13fbddc/comment/jjxg0cy/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button) stating a throw can generate up to 175 Gs.

## Things to Explore

https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le/

https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/BluetoothLE

## IMU Configuration

* deviceId- EEA4022B513B
* date - 638661079975144349
* accXOffset: 0.995585260158052
* accYOffset:  0.988363884404272
* accZOffset:  0.993887001525856
* gyroXOffset:  2.6413647707351
* gyroYOffset: -3.84456837153482
* gyroZOffset: -3.52810129481637




