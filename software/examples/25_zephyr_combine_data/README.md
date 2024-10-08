# Combining Sensor Data

In my previous example I was able to get both the Accelerometer and Gyro data. I made one call to the registers for each data type, 1 for acc and 1 for gyro.

Instead I want to make 1 call and get back all data points.



LSM6DS3 Data Sheet - https://content.arduino.cc/assets/st_imu_lsm6ds3_datasheet.pdf

## My goals

1. Load all data necessary from sensor in one call to the IMU
2. Print that information to the screen.


## Wiring

Same setup as before, connecting SA0 to ground to have address 0x6A.