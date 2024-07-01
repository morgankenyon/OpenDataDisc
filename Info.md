# Info

## MPU 6050 Sensor Info

* 250 deg/s = 131 LSB deg/s
* 500 deg/s = 65.5 LSB deg/s
* 1000 deg/s = 32.8 LSB deg/s
* 2000 deg/s = 16.4 LSB deg/s

https://www.youtube.com/watch?v=yhz3bRQLvBY ~ 4:44 mark

* +/- 2g - 16384 LSB/g
* +/- 4g - 8192 LSB/g
* +/- 8g - 4096 LSB/g
* +/- 16g - 2048 LSB/g

https://www.youtube.com/watch?v=7VW_XVbtu9k ~ 7:53 mark

## Future Sensors

* [High G Single Axis Accelerometer](https://www.sparkfun.com/products/retired/9332)
  * Discontinued, but I could potentially use 2 of these to build a high G sensor.
* [High Performace Gyroscope](https://www.mouser.com/ProductDetail/Analog-Devices/ADXRS649BBGZ?qs=WIvQP4zGanhEKWMUW9AK8A%3D%3D)
  * Maybe combine with above to create my own IMU, idk.
* [200 g sensors](https://www.mouser.com/c/sensors/motion-position-sensors/accelerometers/?acceleration=200%20g)
  * There does seem to be some 200 g sensors that are relatively affordable that I could buy and create my own breadboard for.
* [Replacement for mpu 6050](https://www.mouser.com/ProductDetail/TDK-InvenSense/ICM-42605?qs=gZXFycFWdAO0MPgeewGYjQ%3D%3D)
  * Seems like the mpu 6050 is old

## Battery Info

* Resources
  * https://www.youtube.com/watch?v=vBIE0agqBW0
    * Considerations with UL - https://www.autodesk.com/products/fusion-360/blog/ul-certification-for-electronics-design/
    * A lot of big box stores require UL listings for general safety concerns
    * 

## I2C Article

https://howtomechatronics.com/tutorials/arduino/how-i2c-communication-works-and-how-to-use-it-with-arduino/

## Antenna

This is a similar Antenna to the one used in the TinyPico.

* https://www.mouser.com/ProductDetail/Ignion/NN02-101?qs=ljCeji4nMDmLFkk%252ByszMQQ%3D%3D
* https://www.mouser.com/ProductDetail/Ignion/NN02-101?qs=ljCeji4nMDmLFkk%252ByszMQQ%3D%3D
* https://www.mouser.com/catalog/additional/Ignion_UM_NN02_101.pdf
* Do it yourself: https://www.youtube.com/watch?v=yxU_Kw2de08
* This is the one used in the Tiny Pico, but it's discontinueud: https://mm.digikey.com/Volume0/opasdata/d220001/medias/docus/693/NN01-102_Jan2021.pdf