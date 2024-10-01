#include <zephyr/kernel.h>
#include <zephyr/device.h>
#include <zephyr/devicetree.h>
#include <zephyr/drivers/i2c.h>
//#include <zephyr/smf.h>
#include <zephyr/drivers/gpio.h>


#define I2C_NODE        DT_NODELABEL(arduino_i2c)
static const struct device *i2c_dev = DEVICE_DT_GET(I2C_NODE);


#define IMU_ADDRESS 0x6A

//gyroscope registers
#define LSM6DS3_ACC_GYRO_OUTX_L_G           0X22
#define LSM6DS3_ACC_GYRO_OUTX_H_G           0X23
#define LSM6DS3_ACC_GYRO_OUTY_L_G           0X24
#define LSM6DS3_ACC_GYRO_OUTY_H_G           0X25
#define LSM6DS3_ACC_GYRO_OUTZ_L_G           0X26
#define LSM6DS3_ACC_GYRO_OUTZ_H_G           0X27
//accelerometer registers
#define LSM6DS3_ACC_GYRO_OUTX_L_XL          0X28
#define LSM6DS3_ACC_GYRO_OUTX_H_XL          0X29
#define LSM6DS3_ACC_GYRO_OUTY_L_XL          0X2A
#define LSM6DS3_ACC_GYRO_OUTY_H_XL          0X2B
#define LSM6DS3_ACC_GYRO_OUTZ_L_XL          0X2C
#define LSM6DS3_ACC_GYRO_OUTZ_H_XL          0X2D

int main(void)
{
    return 0;
}
