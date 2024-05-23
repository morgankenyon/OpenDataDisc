# Getting an Accelerometer Working

Now that I've got a blink sketch down, I want to use my accelerometer to get back some real data.

For reference I am using the following components:

* A esp32 microcontroller (ESP32-S3-DevKitC-1-N8)
  * https://www.mouser.com/ProductDetail/Espressif-Systems/ESP32-S3-DevKitC-1-N8?qs=sGAEpiMZZMuqBwn8WqcFUipNgoezRlc4bGSrPQu5tzyJnBQGtrSXSw%3D%3D
* An accelerometer (Adafruit MPU-6050 6-DoF Accel and Gyro Sensor - STEMMA QT Qwiic)
  * https://www.mouser.com/ProductDetail/Adafruit/3886?qs=xZ%2FP%252Ba9zWqYWl0i8uQS6xQ%3D%3D
* A cable to connect the two (STEMMA QT / Qwiic JST SH 4-pin to Premium Male Headers Cable - 150mm Long)
  * https://www.mouser.com/ProductDetail/Adafruit/4209?qs=PzGy0jfpSMvCXPIwCvMoFg%3D%3D

## Installing Libraries

I had to install the following libraries into my Arduino IDE to get this working:

* Adafruit MPU6050
* Adafruit BusIO
* Adafruit Unified Sensor

## Hooking Everything Up

I have the ESP 32 already on my breadboard.

The accelerometer has 2 Qwiic connectors on it, I'm plugging mine into the right side one:

![accelerometer](image.png)

Then connecting the following leads into the following ports on my microcontroller:

I followed [this tutorial](https://randomnerdtutorials.com/esp32-mpu-6050-accelerometer-gyroscope-arduino/) matching the header pins from [this documentation](https://docs.espressif.com/projects/esp-idf/en/latest/esp32s3/hw-reference/esp32s3/user-guide-devkitc-1.html) to hook it up.

* Blue (SDA) => # 6 on board
* Yellow (SCL) => # 5 on board
* Black (Ground) => G
* Red (Power) => 3V3

### Finding the right ports

The tutorial about from randomnerdtutorials has a board where the SDA and SCL default pins are 21 and 22. Looking up the documentation for my board, it's 5 & 6:

![datasheet with pins](image-1.png)

## Issues I encountered

I had several issues I encountered while doing this project:

* Which pins to hook the accelerometer up to.
  * The example I was following used pins 21 & 22.
  * I didn't have pin 22.
  * So I referenced the esp32 documentation (screenshot above), and choose 5 & 6.
* Wrong baud rate for the serial monitor.
  * This sketch prints sensor measurements back to my computer.
  * When you open the serial monitor in Arduino IDE, you have to match the baud rate in code.
  * The following rates need to match:
![baud rates](baudrate.png)
  * No matching, no messages
* When I uploaded my sketch, I got the following error:
  * "Failed to find MPU 6050 chip"
![error](image-2.png)
  * I had to tell the Adafruit library which port I was using.
  * This was accomplished by the following line of code `Wire.begin(6, 5);`, see sketch for reference.
* I might have acceleration and rotation mixed up, more work to do.

Once I fixed all those things, everything worked.