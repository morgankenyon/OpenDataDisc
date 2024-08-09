/*****************************************************************************/
//Trying to sync bluetooth up with Avalonia
//https://wiki.seeedstudio.com/XIAO-BLE-Sense-Bluetooth-Usage/
/*******************************************************************************/

#include "Arduino.h"
#include <ArduinoBLE.h>
#include "LSM6DS3.h"
#include "Wire.h"

//Create a instance of class LSM6DS3
LSM6DS3 myIMU(I2C_MODE, 0x6A);    //I2C device address 0x6A


BLEService nrf52Service("900e9509-a0b2-4d89-9bb6-b5e011e758b0");

BLEStringCharacteristic nrf52Characteristic("6ef4cd45-7223-43b2-b5c9-d13410b494f5", BLERead | BLEWrite | BLENotify | BLEIndicate, 20);
BLEDescriptor nrf52Descriptor("57be2a28-fdf9-4cfd-8bbe-79a86d68765d","NRF52Descriptor");
const int ledPin = LED_BUILTIN; // pin to use for the LED


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

void connectHandler(BLEDevice central) {
  // central connected event handler
  Serial.print("Connected event, central: ");
  Serial.println(central.address());
  digitalWrite(ledPin, HIGH);
}

// listen for BLE disconnect events:
void disconnectHandler(BLEDevice central) {
  // central disconnected event handler
  Serial.print("Disconnected event, central: ");
  Serial.println(central.address());
  digitalWrite(ledPin, LOW);
}

String normalizeSensor(float value)
{
  return String(value, 3);
}

void setup() {
  Serial.begin(125000);

  while (!Serial);
  //Call .begin() to configure the IMUs
  if (myIMU.begin() != 0) {
    Serial.println("Device error");
  } else {
    Serial.println("Device OK!");
  }

  myIMU.settings = CreateSettings();

  // set LED pin to output mode
  //pinMode(ledPin, OUTPUT);

  // begin initialization
  if (!BLE.begin()) {
    Serial.println("starting bluetooth failed");

    while (1);
  }

  BLE.setEventHandler(BLEConnected, connectHandler);
  BLE.setEventHandler(BLEDisconnected, disconnectHandler);

  // set advertised local name and service UUID:
  BLE.setLocalName("NRF52");
  BLE.setAdvertisedService(nrf52Service);

  
  // add the characteristic to the service
  nrf52Service.addCharacteristic(nrf52Characteristic);
  

  // add service
  BLE.addService(nrf52Service);

  Serial.print("Characteristic length: ");
  Serial.println(nrf52Characteristic.valueLength());
  // set the initial value for the characeristic:
  nrf52Characteristic.addDescriptor(nrf52Descriptor);

  // start advertising
  BLE.advertise();

  Serial.println("BLE LED Peripheral");
}

void loop() {
  Serial.println("Trying to listen");
  BLEDevice central = BLE.central();
  String value = "DataDisc";

  // if a central is connected to peripheral:
  if (central) {
    Serial.print("Connected to central: ");
    // print the central's MAC address:
    Serial.println(central.address());

    // while the central is still connected to peripheral:
    while (central.connected()) {
      //Serial.println("writing value");
      
      // accelerometer values
      // float accX = myIMU.readFloatAccelX();
      // float accY = myIMU.readFloatAccelY();
      // float accZ = myIMU.readFloatAccelZ();
      String accX = normalizeSensor(myIMU.readFloatAccelX());
      String accY = normalizeSensor(myIMU.readFloatAccelY());
      String accZ = normalizeSensor(myIMU.readFloatAccelZ());
      String acc = String(accX + "," + accY + "," + accZ);
      // gyro values
      String gyroX = normalizeSensor(myIMU.readFloatGyroX());
      String gyroY = normalizeSensor(myIMU.readFloatGyroY());
      String gyroZ = normalizeSensor(myIMU.readFloatGyroZ());
      String gyro = String(gyroX + "," + gyroY + "," + gyroZ);

      String combinedSensors = String (acc + ":" + gyro);
      Serial.println(combinedSensors);

      nrf52Characteristic.writeValue(combinedSensors);
      delay(1000);
    }

    // when the central disconnects, print it out:
    Serial.print(F("Disconnected from central: "));
    Serial.println(central.address());
  }

  delay(3000);
}