# Bluetooth with Accelerometer

This project will help me connect my accelerometer sensor with my computer.

When connecting to a client, my bluetooth server will take readings from my accelerometer and send them back to windows via bluetooth.

Overall, starting with same code that I had in example 7. And just adding accelerometer readings.

## Esp32 Setup

* Setup platformIO
* Integrate Accelerometer code from example 2
* Upload to board
  * `pio run --target upload`

## Issues

Encountered the following issues building my `main.cpp`:

```
Adafruit_MPU6050.h:21:10: fatal error: Adafruit_BusIO_Register.h: No such file or directory
```

I had to install both the Adafruit dependency in my `platformio.ini`, as well as copy this header `#include <Adafruit_MPU6050.h>` into my `main.cpp`.

I find it a little odd that this code change affected dependency resolution, but who knows the reason.