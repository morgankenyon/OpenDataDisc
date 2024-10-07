# Zephyr & IMU

After using the temperature sensor, now I want to try to use the Accelerometer and Gyro sensors.

Using this library as a starting point and reference: https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3

LSM6DS3 Data Sheet - https://content.arduino.cc/assets/st_imu_lsm6ds3_datasheet.pdf

## My goals

1. Allow easy selection and configuration of Accelerometer and Gyro
2. Provide method to pull data from the accelerometer and gyro
3. Print that data to the screen

### Accelerometer Data

The output of the accelerometer data
```
0: X: -0.018666, Y: -0.106994, Z: 0.979538
1: X: -0.037210, Y: -0.066490, Z: 1.007598
2: X: -0.025498, Y: -0.101260, Z: 0.997594
3: X: -0.050630, Y: -0.120048, Z: 0.977586
4: X: -0.031964, Y: -0.065758, Z: 1.005036
```
## Wiring

Same setup as before, connecting SA0 to ground to have address 0x6A.

## Issues