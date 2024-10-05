#include <zephyr/kernel.h>
#include <zephyr/device.h>
#include <zephyr/devicetree.h>
#include <zephyr/drivers/i2c.h>
//#include <zephyr/smf.h>
#include <zephyr/drivers/gpio.h>

#define SLEEP_TIME_MS           2000
#define I2C_NODE        DT_NODELABEL(arduino_i2c)
static const struct device *i2c_dev = DEVICE_DT_GET(I2C_NODE);

static uint8_t i2c_buffer[2];

//These addresses needs to be associated with the input to the SA0 pin
#define IMU_ADDRESS 0x6A            //imu address when SA0 connected to ground
//#define IMU_ADDRESS 0x6B          //imu address when SA0 connected to VDD

//falling register naming convetion located here: https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/LSM6DS3.h
//WHO_AM_I
#define LSM6DS3_ACC_GYRO_WHO_AM_I_REG       0X0F
//accelerometer control configuration register
#define LSM6DS3_ACC_GYRO_CTRL1_XL  			0X10
//gyro control configuration register
#define LSM6DS3_ACC_GYRO_CTRL2_G  			0X11
//rounding and sensor test status
#define LSM6DS3_ACC_GYRO_CTRL5_C            0X14
//controle register
#define LSM6DS3_ACC_GYRO_CTRL10_C           0X19
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

//read value from the register passed in
uint8_t read_control_register(uint8_t offset)
{
    //set the address to be read as first byte of buffer
    i2c_buffer[0] = offset;
    
    //write that to the i2c
    int err = i2c_write(i2c_dev, i2c_buffer, 1, IMU_ADDRESS);
    if (err < 0)
    {
        printk("control register write failed: %d\n", err);
    }

    //read the byte from the i2c device
    err = i2c_read(i2c_dev, i2c_buffer, 1, IMU_ADDRESS);
    if (err < 0)
    {
        printk("control register read failed: %d\n", err);
    }

    //convert to individual variable and return
    uint8_t controlRegisterValue = (uint8_t)i2c_buffer[0];

    return controlRegisterValue;
}

//write data to the register specified
void write_control_register(uint8_t offset, uint8_t dataToWrite)
{
    //first slot gets the register to write
    i2c_buffer[0] = offset;
    //second slot gets the data
    i2c_buffer[1] = dataToWrite;

    //write 2 bytes to the i2c device
    int err = i2c_write(i2c_dev, i2c_buffer, 2, IMU_ADDRESS);
    if (err < 0)
    {
        printk("control register write failed: %d\n", err);
    }
}

int main(void)
{
    int err;

    //double check that device is ready
    if (!device_is_ready(i2c_dev))
    {
        printk("i2c_dev not ready\n");
        return 0;
    }

    
    //read config value from accelerometer register
    //uint8_t accConfigValue = read_control_register(LSM6DS3_ACC_GYRO_CTRL1_XL);
    //printk("acc control register: %d\n", accConfigValue);

    //configure accelerometer, check datasheet to see what 16 turned to binary means for this register
    uint8_t accControlValue = 16;
    write_control_register(LSM6DS3_ACC_GYRO_CTRL1_XL, accControlValue);

    //confirm write was successful
    uint8_t accUpdatedConfigValue = read_control_register(LSM6DS3_ACC_GYRO_CTRL1_XL);
    printk("updated acc control register: %d\n", accUpdatedConfigValue);

    //read config value from gyro register
    //uint8_t gyroConfigValue = read_control_register(LSM6DS3_ACC_GYRO_CTRL2_G);
    //printk("gyro control register: %d\n", gyroConfigValue);

    //configure gyro, check datasheet to see what 16 turned into binary means for this register
    uint8_t gyroControlValue = 16;
    write_control_register(LSM6DS3_ACC_GYRO_CTRL2_G, gyroControlValue);

    //confirm value is correct
    uint8_t gyroUpdatedConfigValue = read_control_register(LSM6DS3_ACC_GYRO_CTRL2_G);
    printk("updated gyro control register: %d\n", gyroUpdatedConfigValue);

    int count = 0;
    while (true)
    {
        //getting temperature to setting buffer to temperature register
        i2c_buffer[0] = LSM6DS3_ACC_GYRO_OUT_TEMP_L;

        do
        {
            //write the temperature register to device
            err = i2c_write(i2c_dev, i2c_buffer, 1, IMU_ADDRESS);
            if (err < 0)
            {
                printk("write failed: %d\n", err);
                break;
            }

            //both temperature bytes from register
            err = i2c_read(i2c_dev, i2c_buffer, 2, IMU_ADDRESS);
            if (err < 0)
            {
                printk("read failed: %d\n", err);
                break;
            }

            //read raw data from i2c_buffer
            int16_t output = (int16_t)i2c_buffer[0] | (int16_t)(i2c_buffer[1] << 8);

            //Help for temperature calculation taken from: https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/LSM6DS3.cpp
            //temp sensitivity pulled from the above library for the LSM6DS3
            //also found in LSM6DS3 datasheet section 4.3
            float tempC = (float)output / 16.0;
            
            tempC += 25; //default temperature reference, pulled from datasheet

            //convert to F
            float tempF = (tempC * 9) / 5 + 32;

            printk("%d: temperature C: %f, F: %f\n", count, tempC, tempF);
            count++;
        } while (false);

        k_msleep(SLEEP_TIME_MS);
    }
    return 0;
}
