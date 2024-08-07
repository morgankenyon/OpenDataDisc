# The NRF52 IMU

Now that we've got the blink working on the nrf52 Sense. I want to start working on using the accelerometer/gyro. Dropping back to Arduino just to get something working since there are examples for that.

## What I Did

* Install SEEED Boards
  * Follow instructions on their wiki
  * https://wiki.seeedstudio.com/XIAO_BLE/#hardware-setup
* Install the LSM6DS3 arduino library
  * In Arduino IDE
  * Tools > Manage Libraries > Search for "LSM6DS3"
  * Install the following library:
![arduino library to install](image.png)
* Use the code provided in this example
* Upload to board
* Open serial plotter, ensure Baud rate is 115200 (matching what is in the `setup()` method)
* Should see printout of accelerometer values:
![accelerometer values](image-1.png)
  * My board sowed up on either COM4 or COM6. Arduino should let you select the correct one.

## Resources

* Forum Post - https://forum.seeedstudio.com/t/seeed-xiao-ble-nrf52840-sense-giving-bad-gyroscope-data/274134
* SparkFun Documentation - https://learn.sparkfun.com/tutorials/lsm6ds3-breakout-hookup-guide/all#installing-the-arduino-library
* Sparkfun Github Repo - https://github.com/sparkfun/SparkFun_LSM6DS3_Arduino_Library