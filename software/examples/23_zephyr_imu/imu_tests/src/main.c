#include <zephyr/kernel.h>
#include <zephyr/device.h>
#include <zephyr/devicetree.h>
#include <zephyr/drivers/i2c.h>
//#include <zephyr/smf.h>
#include <zephyr/drivers/gpio.h>

#define SLEEP_TIME_MS           1000
#define I2C_NODE        DT_NODELABEL(arduino_i2c)
static const struct device *i2c_dev = DEVICE_DT_GET(I2C_NODE);

static uint8_t i2c_buffer[2];


#define IMU_ADDRESS 0x6A

//falling register naming convetion located here: https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/LSM6DS3.h
//temperature registers
#define LSM6DS3_ACC_GYRO_OUT_TEMP_L         0x20
#define LSM6DS3_ACC_GYRO_OUT_TEMP_H         0x21
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
    int err;

    if (!device_is_ready(i2c_dev))
    {
        printk("i2c_dev not ready\n");
        return 0;
    }

    while (true)
    {
        i2c_buffer[0] = LSM6DS3_ACC_GYRO_OUT_TEMP_L;

        do
        {
            //write to device
            err = i2c_write(i2c_dev, i2c_buffer, 1, IMU_ADDRESS);
            if (err < 0)
            {
                printk("write failed: %d\n", err);
                break;
            }

            //read from device
            err = i2c_read(i2c_dev, i2c_buffer, 2, IMU_ADDRESS);
            if (err < 0)
            {
                printk("read failed: %d\n", err);
                break;
            }

            //calculate temperature with values in i2c_buffer
            int16_t output = (int16_t)i2c_buffer[0] | (int16_t)(i2c_buffer[1] << 8);

            //calculation taken from: https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/LSM6DS3.cpp
            
            //temp sensitivity pulled from the above library for the LSM6DS3
            //also found in LSM6DS3 datasheet section 4.3
            float tempF = (float)output / 16; //divide by tempSensitivity to scale
            tempF += 25; //Add 25 degrees to remove offset
            tempF = (tempF * 9) / 5 + 32;

            printk("read temperature: %f", tempF);
        } while (false);

        k_msleep(SLEEP_TIME_MS);
    }
    return 0;
}
