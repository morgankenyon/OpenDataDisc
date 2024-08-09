# Microcontroller

This project uses a [XIAO nRF52840 Sense](https://www.seeedstudio.com/Seeed-XIAO-BLE-Sense-nRF52840-p-5253.html) as the brains.

It's main purpose is to establish a bluetooth connection between itself and the client, then send over accelerometer and gyro sensor readings.

## Running

To upload and run this on a nrf52 sense:

* Please refer to [platformIO's documentation](https://docs.platformio.org/en/latest/core/index.html) to download the `pio` command line tool.
* Plug in your nrf52.
* Run `pio run --target upload` in the directory containing the `platformio.ini` folder
  * The first time you run it it takes awhile to build everything.
  * Subsequent builds are faster.
* Should upload to your nrf52.

### Debugging

I use [platformIO's Visual Studio plugin ](https://platformio.org/install/ide?install=vscode), which allows me to use the serial debugger if I ever need to see what is going on with my nrf52.
