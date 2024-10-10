# Sending IMU Data Over Bluetooth

In my previous example I was able to pull both the Accelerometer and Gyro data at once.

Now I want to write code that will allow me to send that data over bluetooth to my host machine.

I'll basically be combining code from example 25 and example2/ble_test.

## My goals

1. Send IMU data over to my host machine
2. Allow writing of more than 23 bytes (by default a BLE notify message is limited to 23 bytes)
3. Save the IMU data into the SQLite database.

Todo:
* Determine how to check if bluetooth device is connected before trying to send any notify messages

The new config lines I added to bump up the bluetooth message size.
```
CONFIG_BT_BUF_ACL_RX_SIZE=502
CONFIG_BT_L2CAP_TX_MTU=498
```
## Accomplishment

I'm able to send over about 60 bytes in one bluetooth message, then my C# client can parse that and store it in my SQLite database!!

Let's go!

## Issues

* Building C strings in zephyr
  * https://stackoverflow.com/questions/76546266/float-to-string-in-c-using-nrf-mcu-and-zephyr
* Upping bluetooth byte in zephyr
  * https://devzone.nordicsemi.com/f/nordic-q-a/81860/set-mtu-size-in-zephyr/340755
  * https://lists.zephyrproject.org/g/users/topic/sending_notification_with_mtu/16761407