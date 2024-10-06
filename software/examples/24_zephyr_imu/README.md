# Zephyr & IMU

After using the temperature sensor, now I want to try to use the Accelerometer and Gyro sensors.

Using this library as a starting point and reference: https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3

LSM6DS3 Data Sheet - https://content.arduino.cc/assets/st_imu_lsm6ds3_datasheet.pdf

## My goals

1. Allow easy selection and configuration of Accelerometer and Gyro
2. Provide method to pull data from the accelerometer and gyro
3. Print that data to the screen


## Wiring

Same setup as before, connecting SA0 to ground to have address 0x6A.

## Issues