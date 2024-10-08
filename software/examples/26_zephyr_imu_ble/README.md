# Sending IMU Data Over Bluetooth

In my previous example I was able to pull both the Accelerometer and Gyro data at once.

Now I want to write code that will allow me to send that data over bluetooth to my host machine.

I'll basically be combining code from example 25 and example2/ble_test.

## My goals

1. Send IMU data over to my host machine
2. Allow writing of more than 23 bytes (by default a BLE notify message is limited to 23 bytes)
3. Save the IMU data into the SQLite database.

## Accomplishment
