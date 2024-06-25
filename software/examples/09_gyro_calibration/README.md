# Sensor Calibration

I am currently using a Accelerometer + Gyro sensor to track movements. The only problem is out of the box the sensor isn't measuring correctly.

This example will be based mainly off of [this video](https://www.youtube.com/watch?v=Yh6mYF3VdFQ).

## Basic Idea

* Take a lot of static measurements of my sensor.
* Then take averages of those measurements
* Use those averages calibrate my gyroscope

## Hardware Setup

Using the basic accelerometer setup found in [example2](../02_accelerometer/)

## Software Setup

### Esp32

* Upload to board
  * `pio run --target upload`

### Windows Client

To run: `dotnet build; dotnet run`.

This connects to the esp32, pulls in a bunch of sensor data, then outputs a json file containing the gyro calibration values (x, y, z).