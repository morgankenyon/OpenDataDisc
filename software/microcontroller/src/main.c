#include <zephyr/kernel.h>
#include <zephyr/device.h>
#include <zephyr/devicetree.h>
#include <zephyr/drivers/i2c.h>
#include <zephyr/drivers/gpio.h>

#include <inttypes.h>
#include <stdio.h>

//bluetooth imports
#include <zephyr/bluetooth/bluetooth.h>
#include <zephyr/bluetooth/conn.h>
#include <zephyr/bluetooth/gatt.h>
#include <zephyr/bluetooth/uuid.h>

//General Macros and variables
#define CONNECTED_SLEEP_TIME_MS           50
#define UNCONNECTED_SLEEP_TIME_MS         4000
#define I2C_NODE        DT_NODELABEL(arduino_i2c)
static const struct device *i2c_dev = DEVICE_DT_GET(I2C_NODE);

static uint8_t acc_buffer[6];

/****************** Device specific Macros  ******************/
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


/****************** Bluetooth Macros  ******************/
// UUID of the custom service
#define ODD_SERV_VAL BT_UUID_128_ENCODE(0x900e9509, 0xa0b2, 0x4d89, 0x9bb6, 0xb5e011e758a0)
#define ODD_SERVICE BT_UUID_DECLARE_128(ODD_SERV_VAL)

// UUID of the custom temperature characteristic
#define ODD_SENSOR_CHRC_VAL BT_UUID_128_ENCODE(0x6ef4cd45, 0x7223, 0x43b2, 0xb5c9, 0xd13410b494a5)
#define ODD_SENSOR_CHRC BT_UUID_DECLARE_128(ODD_SENSOR_CHRC_VAL)


/****************** Structs and enums  ******************/
typedef enum AccelerometerSampleRate {
    ACC_SR_POWER_DOWN      = 0x00,
    ACC_SR_13Hz            = 0x10,
    ACC_SR_26Hz            = 0x20,
    ACC_SR_52Hz            = 0x30,
    ACC_SR_104Hz           = 0x40,
    ACC_SR_208Hz           = 0x50,
    ACC_SR_416Hz           = 0x60,
    ACC_SR_833Hz           = 0x70,
    ACC_SR_1660Hz          = 0x80,
    ACC_SR_3330Hz          = 0x90,
    ACC_SR_6660Hz          = 0xA0,
    ACC_SR_13330Hz         = 0xB0,
} AccelerometerSampleRate;

typedef enum AccelerometerScale {
    ACC_SCALE_2G              = 0x00,
    ACC_SCALE_16G             = 0x04,
    ACC_SCALE_4G              = 0x08,
    ACC_SCALE_8G              = 0x0C,
} AccelerometerScale;

typedef enum AccelerometerBandwidth {
    ACC_BANDWIDTH_400HZ     = 0x00,
    ACC_BANDWIDTH_200HZ     = 0x01,
    ACC_BANDWIDTH_100HZ     = 0x02,
    ACC_BANDWIDTH_50HZ      = 0x03,
} AccelerometerBandwidth;

typedef enum GyroSampleRate {
    GYRO_SR_POWER_DOWN     = 0x00,
    GYRO_SR_13Hz           = 0x10,
    GYRO_SR_26Hz           = 0x20,
    GYRO_SR_52Hz           = 0x30,
    GYRO_SR_104Hz          = 0x40,
    GYRO_SR_208Hz          = 0x50,
    GYRO_SR_416Hz          = 0x60,
    GYRO_SR_833Hz          = 0x70,
    GYRO_SR_1660Hz         = 0x80,
} GyroSampleRate;

typedef enum GyroScale {
    GYRO_SCALE_250dps       = 0x00,
    GYRO_SCALE_500dps       = 0x04,
    GYRO_SCALE_1000dps      = 0x08,
    GYRO_SCALE_2000dps      = 0x0C,
} GyroScale;

typedef enum GyroFullScale {
    GYRO_FULLSCALE_125dps_DISABLED      = 0x00,
    GYRO_FULLSCALE_125dps_ENABLED       = 0x02,
} GyroFullScale;

typedef enum OperationMode {
    THROW           = 0x00,
    CONFIGURE       = 0x02
} OperationMode;

typedef struct IMUSettings
{
    AccelerometerSampleRate accSampleRate;
    AccelerometerScale accScale;
    AccelerometerBandwidth accBandwidth;
    GyroSampleRate gyroSampleRate;
    GyroScale gyroScale;
    GyroFullScale gyroFullScale;
} IMUSettings;

typedef struct AcceloremeterData
{
    float AccX;
    float AccY;
    float AccZ;
} AcceloremeterData;

typedef struct GyroData
{
    float GyroX;
    float GyroY;
    float GyroZ;
} GyroData;

typedef struct IMUData
{
    AcceloremeterData aData;
    GyroData gData;
    uint32_t cycleCount;
} IMUData;

enum OperationMode operationMode;


/************* Bluetooth Info **************/
volatile bool ble_ready=false;
volatile bool ble_connected=false;

static const struct bt_data ad[] = 
{
    BT_DATA_BYTES(BT_DATA_FLAGS, (BT_LE_AD_GENERAL | BT_LE_AD_NO_BREDR)),
    BT_DATA_BYTES(BT_DATA_UUID128_ALL, ODD_SERV_VAL)
};


//used to read messages written to the server from the client
static ssize_t on_write(struct bt_conn *conn, const struct bt_gatt_attr *attr,
                        const void *buf, uint16_t len, uint16_t offset, uint8_t flags)
{
    char received_data[100];  // Adjust size based on your expected data length

    printk("In on_write\n");
    // Ensure the length does not exceed buffer size
    if (len >= sizeof(received_data)) {
        printk("Error: Data too long to fit into buffer\n");
        return BT_GATT_ERR(BT_ATT_ERR_INVALID_ATTRIBUTE_LEN);
    }

    // Copy the incoming bytes to the string buffer and add null termination
    memcpy(received_data, buf, len);
    received_data[len] = '\0';  // Null-terminate the string

    // Print or process the received string
    printk("Received data as string: %s\n", received_data);

    // Return the number of bytes written
    return len;
}

BT_GATT_SERVICE_DEFINE(custom_srv,
	BT_GATT_PRIMARY_SERVICE(ODD_SERVICE),
	BT_GATT_CHARACTERISTIC(ODD_SENSOR_CHRC, 
        BT_GATT_CHRC_NOTIFY | BT_GATT_CHRC_WRITE | BT_GATT_CHRC_WRITE_WITHOUT_RESP,
        BT_GATT_PERM_WRITE,
        NULL,
        on_write,
        NULL),
	BT_GATT_CCC(NULL, BT_GATT_PERM_READ | BT_GATT_PERM_WRITE),
);
/********************* Utility Functions  *****************/
/*
 * Gets uptime of system in milliseconds
 * Since Zephyr is an embedded system, can't really get time since epoch
 * For sensor time intervals, I don't need absolute time, just relative
 * time between sensor readings, so this will suffice
 */
uint32_t get_cycle_count()
{
    return k_cycle_get_32();
}


//read value from the register passed in
uint8_t read_control_register(uint8_t offset)
{
    //set the address to be read as first byte of buffer
    acc_buffer[0] = offset;
    
    //write that to the i2c
    int err = i2c_write(i2c_dev, acc_buffer, 1, IMU_ADDRESS);
    if (err < 0)
    {
        printk("control register write failed: %d\n", err);
    }

    //read the byte from the i2c device
    err = i2c_read(i2c_dev, acc_buffer, 1, IMU_ADDRESS);
    if (err < 0)
    {
        printk("control register read failed: %d\n", err);
    }

    //convert to individual variable and return
    uint8_t controlRegisterValue = (uint8_t)acc_buffer[0];

    return controlRegisterValue;
}

//write data to the register specified
void write_control_register(uint8_t offset, uint8_t dataToWrite)
{
    //first slot gets the register to write
    acc_buffer[0] = offset;
    //second slot gets the data
    acc_buffer[1] = dataToWrite;

    //write 2 bytes to the i2c device
    int err = i2c_write(i2c_dev, acc_buffer, 2, IMU_ADDRESS);
    if (err < 0)
    {
        printk("control register write failed: %d\n", err);
    }
}



/********************* Bluetooth Code  *****************/
void bt_ready(int err)
{
	if (err)
	{
		printk("bt enable returns %d", err);
	}

	printk("bt_ready!\n");
	ble_ready = true;
}

int init_ble(void)
{
    int err;

    err = bt_enable(bt_ready);

    if (err)
    {
        printk("Also an error");
        printk("bt_enable failed (err %d)", err);
        return err;
    }

    return 0;
}

static int notify_adc(IMUData data)
{
    //used to calculate the length needed to create the buffer
    int len = snprintf(NULL, 0, "%f,%f,%f;%f,%f,%f;%" PRIu32,
        data.aData.AccX,
        data.aData.AccX,
        data.aData.AccZ,
        data.gData.GyroX,
        data.gData.GyroY,
        data.gData.GyroZ,
        data.cycleCount);

    //Different number of floats led to different values here than I would expect
    //need to dig optimizing this
    char buffer[len];
    snprintf(buffer, sizeof buffer, "%f,%f,%f;%f,%f,%f;%" PRIu32,
        data.aData.AccX,
        data.aData.AccY,
        data.aData.AccZ,
        data.gData.GyroX,
        data.gData.GyroY,
        data.gData.GyroZ,
        data.cycleCount);

    int rc;
    rc = bt_gatt_notify(NULL, &custom_srv.attrs[2], &buffer, sizeof(buffer));

    return rc == -ENOTCONN ? 0 : rc;
}

/*
* Callback to run whenever a bluetooth connection occurs
*/
static void connected(struct bt_conn *conn, uint8_t conn_err)
{
    printk("connected\n");
    ble_connected = true;
}

/*
* Callback to run whenever a bluetooth disconnection occurs
*/
static void disconnected(struct bt_conn *conn, uint8_t reason)
{
    printk("disconnected\n");
    ble_connected = false;
}

BT_CONN_CB_DEFINE(conn_callbacks) = {
	.connected = connected,
	.disconnected = disconnected
};
/******* /Bluetooth Code ********/


/********************* Sensor Code  *****************/

//configure IMU to specified settings
//sets the IMU control registers based on based in values
void configure_imu(IMUSettings settings)
{
    uint8_t dataToWrite = 0;

    //configuring accelerometer
    //each of these settings is stored in different bits of the register
    //so can OR them each individually and they'll set the correct bits
    //The LSM6DS3_ACC_GYRO_CTRL1_XL register is configured as follows:
    //first 4 bits:sampleRate
    //next 2 bits: scale
    //next 2 bits: bandwidth
    //this is reading left to right from the datasheet
    //Right now don't quite know which bit is MSB and LSB.
    dataToWrite |= settings.accBandwidth;
    dataToWrite |= settings.accScale;
    dataToWrite |= settings.accSampleRate;

    //write accelerometer config to register
    write_control_register(LSM6DS3_ACC_GYRO_CTRL1_XL, dataToWrite);

    //might still need to worry about the CTRL4_C register, not 100% sure at this point.

    //reset
    dataToWrite = 0;

    //configuring gyroscope
    //much like the accerelometer, each of these settings is stored
    //in different bits of the register. So we can OR them and they'll
    //set the correct bits.
    //The LSM6DS3_ACC_GYRO_CTRL2_G register is configured as follows
    //first 4 bits: sampleRate
    //next 2 bits: scale
    //next 1 bit: fullScale (really continuation of scale I believe)
    //last bit: 0 (never changes)
    //again, not 100% sure which bit is MSB and LSB
    dataToWrite |= settings.gyroFullScale;
    dataToWrite |= settings.gyroScale;
    dataToWrite |= settings.gyroSampleRate;

    //write gyro to register
    //TODO: This control register also controls the rotation scale, will do this later
    write_control_register(LSM6DS3_ACC_GYRO_CTRL2_G, dataToWrite);

    //reset
    dataToWrite = 0;
}

//uses IMUSettings to turn raw sensor values into correct values
AcceloremeterData calibrate_raw_accelerometer_values(IMUSettings settings, uint8_t startingIndex)
{
    float constant = 0.061;
    int scale = 1;
    switch (settings.accScale)
    {
        case ACC_SCALE_2G:
            scale = 0;
            break;
        case ACC_SCALE_4G:
            scale = 1;
            break;
        case ACC_SCALE_8G:
            scale = 2;
            break;
        case ACC_SCALE_16G:
            scale = 3;
            break;
    }

    float scaledConstant = constant * (1 << scale);

    //parse out different values from 6 byte buffer
    int16_t xOutput = (int16_t)acc_buffer[startingIndex + 0] | (int16_t)(acc_buffer[startingIndex + 1] << 8);
    int16_t yOutput = (int16_t)acc_buffer[startingIndex + 2] | (int16_t)(acc_buffer[startingIndex + 3] << 8);
    int16_t zOutput = (int16_t)acc_buffer[startingIndex + 4] | (int16_t)(acc_buffer[startingIndex + 5] << 8);

    float accX = (float)xOutput * scaledConstant / 1000;
    float accY = (float)yOutput * scaledConstant / 1000;
    float accZ = (float)zOutput * scaledConstant / 1000;

    struct AcceloremeterData data;
    data.AccX = accX;
    data.AccY = accY;
    data.AccZ = accZ;


    return data;
}

//take raw gyro inputs and output calibrated values
GyroData calibrate_raw_gyro_values(IMUSettings settings, uint8_t startingIndex)
{
    int scale = 1;
    switch (settings.gyroScale)
    {
        //Don't have 125dps, probably won't need it for this project
        // case GYRO_SCALE_250dps:
        //     scale = 0;
        //     break;
        case GYRO_SCALE_250dps:
            scale = 250;
            break;
        case GYRO_SCALE_500dps:
            scale = 500;
            break;
        case GYRO_SCALE_1000dps:
            scale = 1000;
            break;
        case GYRO_SCALE_2000dps:
            scale = 2000;
            break;
    }
    uint8_t gyroRangeDivisor = scale / 125;

    int16_t xOutput = (int16_t)acc_buffer[startingIndex + 0] | (int16_t)(acc_buffer[startingIndex + 1] << 8);
    int16_t yOutput = (int16_t)acc_buffer[startingIndex + 2] | (int16_t)(acc_buffer[startingIndex + 3] << 8);
    int16_t zOutput = (int16_t)acc_buffer[startingIndex + 4] | (int16_t)(acc_buffer[startingIndex + 5] << 8);
    
    float gyroX = (float)xOutput * 4.375 * (gyroRangeDivisor) / 1000;
    float gyroY = (float)yOutput * 4.375 * (gyroRangeDivisor) / 1000;
    float gyroZ = (float)zOutput * 4.375 * (gyroRangeDivisor) / 1000;

    struct GyroData data;
    data.GyroX = gyroX;
    data.GyroY = gyroY;
    data.GyroZ = gyroZ;

    return data;
}

//uses settings to read both the Gyro and accelerometer data from the IMU
IMUData pull_imu_data(IMUSettings settings)
{
    int err;
    acc_buffer[0] = LSM6DS3_ACC_GYRO_OUTX_L_G;

    //write the X value register to device
    err = i2c_write(i2c_dev, acc_buffer, 1, IMU_ADDRESS);
    if (err < 0)
    {
        printk("write failed: %d\n", err);
    }

    //read both gyro and accelerometer sensor data from registers
    //first 6 bytes are gyro, second 6 bytesa re acceleremeter
    err = i2c_read(i2c_dev, acc_buffer, 12, IMU_ADDRESS);
    if (err < 0)
    {
        printk("read failed: %d\n", err);
    }
    //place as close as I can to actual interaction with my IMU
    //not sure if placing it before might be more accurate
    uint32_t cycle_count = get_cycle_count();

    //get gyro data
    struct GyroData gdata = calibrate_raw_gyro_values(settings, 0);
    struct AcceloremeterData adata = calibrate_raw_accelerometer_values(settings, 6);

    struct IMUData imuData;
    imuData.gData = gdata;
    imuData.aData = adata;
    imuData.cycleCount = cycle_count;

    return imuData;
}

int main(void)
{
    //double check that device is ready
    if (!device_is_ready(i2c_dev))
    {
        printk("i2c_dev not ready\n");
        return 0;
    }

    init_ble();

    while (!ble_ready)
    {
        printk("BLE stack not ready yet\n");
        k_msleep(100);
    }
    printk("BLE stack ready\n");

    operationMode = THROW;

    int err;
    err = bt_le_adv_start(BT_LE_ADV_CONN_NAME, ad, ARRAY_SIZE(ad), NULL, 0);
    if (err)
    {
        printk("Advertising failed to start (err %d)\n", err);
        return 1;
    }

    struct IMUSettings settings;
    settings.accSampleRate = ACC_SR_13330Hz;
    settings.accScale = ACC_SCALE_4G;
    settings.accBandwidth = ACC_BANDWIDTH_50HZ;
    settings.gyroSampleRate = GYRO_SR_1660Hz;
    settings.gyroScale = GYRO_SCALE_250dps;
    settings.gyroFullScale = GYRO_FULLSCALE_125dps_DISABLED;

    configure_imu(settings);

    int count = 0;

    while (true)
    {
        if (ble_connected)
        {
            struct IMUData imuData = pull_imu_data(settings);

            printk("%d: Writing to bluetooth service\n", count);
            err = notify_adc(imuData);
            if (err)
            {
                printk("Writing failed (err %d)\n", err);
                return 1;
            }
            count++;

            k_msleep(CONNECTED_SLEEP_TIME_MS);
        }
        else
        {
            k_msleep(UNCONNECTED_SLEEP_TIME_MS);
        }
    }
    return 0;
}
