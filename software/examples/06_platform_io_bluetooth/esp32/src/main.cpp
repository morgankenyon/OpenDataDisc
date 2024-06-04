/*********
  Rui Santos
  Complete instructions at https://RandomNerdTutorials.com/esp32-ble-server-client/
  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files.
  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*********/

/***
 * Morgan Kenyon
 * Trying out example in platformIO
 * 
 */

#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
#include <Wire.h>
#include <Arduino.h>


//BLE server name
#define bleServerName "ESP32_Server"

float num;

// Timer variables
unsigned long lastTime = 0;
unsigned long lastWriteOperation = 0;
unsigned long timerDelay = 3000;

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

class CharacterCallbacks: public BLECharacteristicCallbacks {
  void onWrite(BLECharacteristic* pCharacteristic) {
      std::string rxValue = pCharacteristic->getValue();
      Serial.print("value received = ");
      Serial.println(rxValue.c_str());
      lastWriteOperation = millis();
  }
};

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

  // Init BME Sensor

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
  bmeNumberCharacteristics.setCallbacks(new CharacterCallbacks());

  
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

//   if (deviceConnected && ((millis() - lastWriteOperation) > 20000)) {
//     //bmeService->stop();
//     //bmeService->start();
//     BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
//     pAdvertising->addServiceUUID(SERVICE_UUID);
//     pServer->getAdvertising()->start();
//     deviceConnected = false;
//   }
  delay(1000);
}
