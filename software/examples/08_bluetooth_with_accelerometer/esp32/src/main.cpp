/*********
  Rui Santos
  Complete instructions at https://RandomNerdTutorials.com/esp32-ble-server-client/
  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files.
  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*********/

/***
 * Morgan Kenyon
 * Trying out example in platformIO, example 8
 * 
 */

#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
#include <Wire.h>
#include <Arduino.h>
#include <Adafruit_MPU6050.h>

Adafruit_MPU6050 mpu;
//BLE server name
#define bleServerName "ESP32_Server"

float num;

// Timer variables
//unsigned long lastTime = 0;
//unsigned long lastWriteOperation = 0;
//unsigned long timerDelay = 3000;

bool deviceConnected = false;
bool oldDeviceConnected = false;//added

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/
#define SERVICE_UUID "91bad492-b950-4226-aa2b-4ede9fa42f59"

BLEServer *pServer = NULL;//added
BLEService *bmeService = NULL;

// Number Characteristic and Descriptor
BLECharacteristic bmeNumberCharacteristics(
  "cba1d466-344c-4be3-ab3f-189f80dd7518", 
  BLECharacteristic::PROPERTY_NOTIFY | 
  BLECharacteristic::PROPERTY_WRITE);
BLEDescriptor bmeNumberDescriptor(BLEUUID((uint16_t)0x2902));

//https://stackoverflow.com/a/75943324
static void my_gatts_event_handler(esp_gatts_cb_event_t event, esp_gatt_if_t gatts_if, esp_ble_gatts_cb_param_t* param) {
    // Do stuff depending on event type
    // Eg. 'listen' to GATT events in realtime (very star trek).
    // tone(2000 + event*200, volume, delay);
    
    Serial.println("event");
}

//Setup callbacks onConnect and onDisconnect
class MyServerCallbacks: public BLEServerCallbacks {
  void onConnect(BLEServer* pServer) {
    Serial.println("connected");
    deviceConnected = true;
  };
  void onDisconnect(BLEServer* pServer) {
    Serial.println("disconnected");
    deviceConnected = false;
  }
};

// class CharacterCallbacks: public BLECharacteristicCallbacks {
//   void onWrite(BLECharacteristic* pCharacteristic) {
//       std::string rxValue = pCharacteristic->getValue();
//       Serial.print("value received = ");
//       Serial.println(rxValue.c_str());
//       //lastWriteOperation = millis();
//   }
// };

//https://forum.arduino.cc/t/esp32-ble-does-not-disconnect/957235
void checkToReconnect() //added
{
  // disconnected so advertise
  if (!deviceConnected && oldDeviceConnected) {
    delay(500); // give the bluetooth stack the chance to get things ready
    pServer->startAdvertising(); // restart advertising
    Serial.println("Disconnected: start advertising");
    oldDeviceConnected = deviceConnected;
  }
  // connected so reset boolean control
  if (deviceConnected && !oldDeviceConnected) {
    // do stuff here on connecting
    Serial.println("Reconnected");
    oldDeviceConnected = deviceConnected;
  }
}

void setup() {
  // Start serial communication 
  Serial.begin(115200);
  Wire.begin(6, 5);

  // Init BME Sensor
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip");
    while (1) {
      delay(10);
    }
  }
  Serial.println("MPU6050 Found!");

  //accelerometer setup
  mpu.setAccelerometerRange(MPU6050_RANGE_8_G);
  Serial.print("Accelerometer range set to: ");
  switch (mpu.getAccelerometerRange()) {
  case MPU6050_RANGE_2_G:
    Serial.println("+-2G");
    break;
  case MPU6050_RANGE_4_G:
    Serial.println("+-4G");
    break;
  case MPU6050_RANGE_8_G:
    Serial.println("+-8G");
    break;
  case MPU6050_RANGE_16_G:
    Serial.println("+-16G");
    break;
  }
  mpu.setGyroRange(MPU6050_RANGE_500_DEG);
  Serial.print("Gyro range set to: ");
  switch (mpu.getGyroRange()) {
  case MPU6050_RANGE_250_DEG:
    Serial.println("+- 250 deg/s");
    break;
  case MPU6050_RANGE_500_DEG:
    Serial.println("+- 500 deg/s");
    break;
  case MPU6050_RANGE_1000_DEG:
    Serial.println("+- 1000 deg/s");
    break;
  case MPU6050_RANGE_2000_DEG:
    Serial.println("+- 2000 deg/s");
    break;
  }

  mpu.setFilterBandwidth(MPU6050_BAND_5_HZ);
  Serial.print("Filter bandwidth set to: ");
  switch (mpu.getFilterBandwidth()) {
  case MPU6050_BAND_260_HZ:
    Serial.println("260 Hz");
    break;
  case MPU6050_BAND_184_HZ:
    Serial.println("184 Hz");
    break;
  case MPU6050_BAND_94_HZ:
    Serial.println("94 Hz");
    break;
  case MPU6050_BAND_44_HZ:
    Serial.println("44 Hz");
    break;
  case MPU6050_BAND_21_HZ:
    Serial.println("21 Hz");
    break;
  case MPU6050_BAND_10_HZ:
    Serial.println("10 Hz");
    break;
  case MPU6050_BAND_5_HZ:
    Serial.println("5 Hz");
    break;
  }

  //bluetooth setup
  // Create the BLE Device\
  BLEDevice::setCustomGattsHandler(my_gatts_event_handler);
  BLEDevice::init(bleServerName);

  // Create the BLE Server
  BLEServer *pServer = BLEDevice::createServer();
  //*pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Create the BLE Service
  BLEService *bmeService = pServer->createService(SERVICE_UUID);

  // Create BLE Characteristics and Create a BLE Descriptor
  // Number
  bmeService->addCharacteristic(&bmeNumberCharacteristics);
  bmeNumberDescriptor.setValue("BME Number");
  bmeNumberCharacteristics.addDescriptor(&bmeNumberDescriptor);
  //bmeNumberCharacteristics.setCallbacks(new CharacterCallbacks());

  
  // Start the service
  bmeService->start();

  // Start advertising
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pServer->getAdvertising()->start();
  Serial.println("Waiting a client connection to notify...");
}

void loop() {
  checkToReconnect();
  
  if (deviceConnected) {
    //read accelerometer data
    sensors_event_t a, g, temp;
    mpu.getEvent(&a, &g, &temp);

    //acceleration measurements
    //following https://forum.arduino.cc/t/concatenating-multiple-float-variables-with-limiter-to-char-array/311300/5
    char accX[7];
    char accY[7];
    char accZ[7];

    dtostrf(a.acceleration.x, 6, 2, accX);
    dtostrf(a.acceleration.y, 6, 2, accY);
    dtostrf(a.acceleration.z, 6, 2, accZ);

    char rotX[7];
    char rotY[7];
    char rotZ[7];
    dtostrf(g.gyro.x, 6, 2, rotX);
    dtostrf(g.gyro.y, 6, 2, rotY);
    dtostrf(g.gyro.z, 6, 2, rotZ);

    char sensorBuffer[strlen(accX) + strlen(accY) + strlen(accZ) + strlen(rotX) + strlen(rotY) + strlen(rotZ) + 5];

    sprintf(sensorBuffer, "Acc:%s,%s,%s, Rot:%s,%s,%s", accX, accY, accZ, rotX, rotY, rotZ);

    bmeNumberCharacteristics.setValue(sensorBuffer);
    bmeNumberCharacteristics.notify();

    // //old notify code
    // num = num + 1;

    // //Notify number reading
    // static char numberTemp[6];
    // dtostrf(num, 6, 2, numberTemp);
    // //Set Number Characteristic value and notify connected client
    // // bmeNumberCharacteristics.setValue(numberTemp);
    // // bmeNumberCharacteristics.notify();
    // Serial.print("Number: ");
    // Serial.println(num);
    
    
    // lastTime = millis();
  }
  else
  {
    checkToReconnect();
    delay(1000);
  }

  delay(500);
}
