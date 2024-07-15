/*****************************************************************************/
//Ensure values received from the IMU sensor are calibrated correctly
/*******************************************************************************/

#include "Arduino.h"
#include "LSM6DS3.h"
#include "Wire.h"

//Create a instance of class LSM6DS3
LSM6DS3 myIMU(I2C_MODE, 0x6A);    //I2C device address 0x6A


SensorSettings CreateSettings()
{
    SensorSettings settings = SensorSettings();
    //copied from Seeed library
    //https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/LSM6DS3.cpp#L354
    settings.gyroEnabled = 1;  //Can be 0 or 1

    settings.gyroRange = 2000;   //Max deg/s.  Can be: 125, 245, 500, 1000, 2000
    settings.gyroSampleRate = 416;   //Hz.  Can be: 13, 26, 52, 104, 208, 416, 833, 1666
    settings.gyroBandWidth = 400;  //Hz.  Can be: 50, 100, 200, 400;
    settings.gyroFifoEnabled = 1;  //Set to include gyro in FIFO
    settings.gyroFifoDecimation = 1;  //set 1 for on /1

    settings.accelEnabled = 1;
    settings.accelODROff = 1;
    settings.accelRange = 16;      //Max G force readable.  Can be: 2, 4, 8, 16
    settings.accelSampleRate = 416;  //Hz.  Can be: 13, 26, 52, 104, 208, 416, 833, 1666, 3332, 6664, 13330
    settings.accelBandWidth = 100;  //Hz.  Can be: 50, 100, 200, 400;
    settings.accelFifoEnabled = 1;  //Set to include accelerometer in the FIFO
    settings.accelFifoDecimation = 1;  //set 1 for on /1

    settings.tempEnabled = 0;

    //Select interface mode
    settings.commMode = 1;  //Can be modes 1, 2 or 3

    //FIFO control data
    settings.fifoThreshold = 3000;  //Can be 0 to 4096 (16 bit bytes)
    settings.fifoSampleRate = 10;  //default 10Hz
    settings.fifoModeWord = 0;  //Default off

    return settings;
}

void setup() {
    // put your setup code here, to run once:
    Serial.begin(115200);
    while (!Serial);
    //Call .begin() to configure the IMUs
    if (myIMU.begin() != 0) {
        Serial.println("Device error");
    } else {
        Serial.println("Device OK!");
    }

    myIMU.settings = CreateSettings();
}

void loop() {
    //Accelerometer
    float accX = myIMU.readFloatAccelX();
    float accY = myIMU.readFloatAccelY();
    float accZ = myIMU.readFloatAccelZ();
    Serial.print("Accelerometer:");
    Serial.print(" X = ");
    Serial.print(accX, 4);

    Serial.print(", Y = ");
    Serial.print(accY, 4);

    Serial.print(", Z = ");
    Serial.println(accZ, 4);

    //Gyroscope
    float gyroX = myIMU.readFloatGyroX();
    float gyroY = myIMU.readFloatGyroY();
    float gyroZ = myIMU.readFloatGyroZ();
    Serial.print("Gyroscope:");
    Serial.print(" X = ");
    Serial.print(gyroX, 4);

    Serial.print(", Y = ");
    Serial.print(gyroY, 4);

    Serial.print(", Z = ");
    Serial.println(gyroZ, 4);

    delay(1000);
}
