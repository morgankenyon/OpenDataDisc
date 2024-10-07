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

### Gyro Data

The output of the gyro data

```
0: Gyro - X: 2.310000, Y: -3.990000, Z: -3.780000
1: Gyro - X: 2.450000, Y: -3.710000, Z: -3.920000
2: Gyro - X: 2.520000, Y: -3.850000, Z: -4.200000
3: Gyro - X: 2.520000, Y: -3.850000, Z: -3.850000
4: Gyro - X: 2.520000, Y: -3.850000, Z: -3.990000
5: Gyro - X: 1.960000, Y: -3.850000, Z: -4.130000
6: Gyro - X: 2.380000, Y: -4.340000, Z: -3.850000
7: Gyro - X: 1.820000, Y: -3.990000, Z: -3.850000
8: Gyro - X: 2.310000, Y: -4.200000, Z: -4.060000
9: Gyro - X: 2.240000, Y: -4.060000, Z: -3.920000
```

If I remember correctly I still need to calibrate these values. So this is a stationary and I'll need to average values out so it gets close to zero.

### Combined Data Output

The combined data output:

```
0: Acc - X: -0.053924, Y: -0.034770, Z: 1.000522
0: Gyro - X: 2.485000, Y: -3.701250, Z: -4.331250
1: Acc - X: -0.013298, Y: -0.090524, Z: 0.975634
1: Gyro - X: 2.572500, Y: -3.797500, Z: -3.797500
2: Acc - X: -0.019032, Y: -0.066246, Z: 0.979416
2: Gyro - X: 2.266250, Y: -3.815000, Z: -3.517500
3: Acc - X: -0.049776, Y: -0.045994, Z: 1.015528
3: Gyro - X: 1.863750, Y: -3.543750, Z: -3.482500
4: Acc - X: -0.042822, Y: -0.047214, Z: 1.004792
4: Gyro - X: 2.345000, Y: -3.762500, Z: -3.815000
5: Acc - X: -0.015372, Y: -0.090890, Z: 0.987102
5: Gyro - X: 2.397500, Y: -3.570000, Z: -3.535000
```

## Wiring

Same setup as before, connecting SA0 to ground to have address 0x6A.