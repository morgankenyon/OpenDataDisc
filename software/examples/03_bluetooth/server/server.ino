/*********
  Rui Santos
  Complete instructions at https://RandomNerdTutorials.com/esp32-ble-server-client/
  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files.
  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*********/

/***
 * Morgan Kenyon
 * Server Code used to push notifications to our client
 * I used a Esp32 S3 Dev board for this code
 * 
 */

#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
#include <Wire.h>

//BLE server name
#define bleServerName "ESP32_Server"

float num;

// Timer variables
unsigned long lastTime = 0;
unsigned long timerDelay = 10000;

bool deviceConnected = false;

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/
#define SERVICE_UUID "91bad492-b950-4226-aa2b-4ede9fa42f59"

// Number Characteristic and Descriptor
BLECharacteristic bmeNumberCharacteristics("cba1d466-344c-4be3-ab3f-189f80dd7518", BLECharacteristic::PROPERTY_NOTIFY);
BLEDescriptor bmeNumberDescriptor(BLEUUID((uint16_t)0x2902));


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


void setup() {
  // Start serial communication 
  Serial.begin(115200);

  // Init BME Sensor

  // Create the BLE Device
  BLEDevice::init(bleServerName);

  // Create the BLE Server
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Create the BLE Service
  BLEService *bmeService = pServer->createService(SERVICE_UUID);

  // Create BLE Characteristics and Create a BLE Descriptor
  // Number
  bmeService->addCharacteristic(&bmeNumberCharacteristics);
  bmeNumberDescriptor.setValue("BME Number");
  bmeNumberCharacteristics.addDescriptor(&bmeNumberDescriptor);

  
  // Start the service
  bmeService->start();

  // Start advertising
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pServer->getAdvertising()->start();
  Serial.println("Waiting a client connection to notify...");
}

void loop() {
  if (deviceConnected) {
    if ((millis() - lastTime) > timerDelay) {
      num = num + 1;
  
      //Notify number reading
      static char numberTemp[6];
      dtostrf(num, 6, 2, numberTemp);
      //Set Number Characteristic value and notify connected client
      bmeNumberCharacteristics.setValue(numberTemp);
      bmeNumberCharacteristics.notify();
      Serial.print("Number: ");
      Serial.println(num);
      
      
      lastTime = millis();
    }
  }
}
