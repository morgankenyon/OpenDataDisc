# Combining Sensor Data

In my previous example I was able to get both the Accelerometer and Gyro data. I made one call to the registers for each data type, 1 for acc and 1 for gyro.

Instead I want to make 1 call and get back all data points.

LSM6DS3 Data Sheet - https://content.arduino.cc/assets/st_imu_lsm6ds3_datasheet.pdf

## My goals

1. Load all data necessary from sensor in one call to the IMU
2. Print that information to the screen.

## Accomplishment

I was able to get both data sets in one interaction with the IMU
```
56000: Acc - X: -0.004514, Y: -0.087108, Z: 0.979538
56000: Gyro - X: 2.598750, Y: -3.508750, Z: -3.710000
56500: Acc - X: -0.048068, Y: -0.032452, Z: 1.012478
56500: Gyro - X: 2.432500, Y: -3.815000, Z: -3.963750
57000: Acc - X: -0.050020, Y: -0.049166, Z: 1.023702
57000: Gyro - X: 2.380000, Y: -3.902500, Z: -4.121250
57500: Acc - X: -0.043798, Y: -0.057340, Z: 1.016504
57500: Gyro - X: 2.572500, Y: -3.876250, Z: -3.902500
58000: Acc - X: -0.007808, Y: -0.050752, Z: 0.982954
58000: Gyro - X: 2.546250, Y: -3.850000, Z: -3.718750
```

## Wiring

Same setup as before, connecting SA0 to ground to have address 0x6A.