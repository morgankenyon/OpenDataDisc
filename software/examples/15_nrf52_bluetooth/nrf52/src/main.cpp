/*****************************************************************************/
//Trying to get bluetooth working
//https://wiki.seeedstudio.com/XIAO-BLE-Sense-Bluetooth-Usage/
/*******************************************************************************/

#include "Arduino.h"
#include <ArduinoBLE.h>
#include "Wire.h"

BLEService nrf52Service("900e9509-a0b2-4d89-9bb6-b5e011e758b0");

BLECharacteristic nrf52Characteristic("6ef4cd45-7223-43b2-b5c9-d13410b494f5", BLERead | BLEWrite | BLENotify | BLEIndicate, "nrf52Characteristic");
BLEDescriptor nrf52Descriptor("57be2a28-fdf9-4cfd-8bbe-79a86d68765d","NRF52Descriptor");
const int ledPin = LED_BUILTIN; // pin to use for the LED

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

void setup() {
  Serial.begin(125000);
  while (!Serial);

  // set LED pin to output mode
  pinMode(ledPin, OUTPUT);

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

  // set the initial value for the characeristic:
  nrf52Characteristic.addDescriptor(nrf52Descriptor);

  // start advertising
  BLE.advertise();

  Serial.println("BLE LED Peripheral");
}

void loop() {
  Serial.println("Trying to listen");
  BLEDevice central = BLE.central();
  byte outgoing[8];
  outgoing[0] = 0x44; //D
  outgoing[1] = 0x61; //a
  outgoing[2] = 0x74; //t
  outgoing[3] = 0x61; //a
  outgoing[4] = 0x44; //D
  outgoing[5] = 0x69; //i
  outgoing[6] = 0x73; //s
  outgoing[7] = 0x63; //c

  // if a central is connected to peripheral:
  if (central) {
    Serial.print("Connected to central: ");
    // print the central's MAC address:
    Serial.println(central.address());

    // while the central is still connected to peripheral:
    while (central.connected()) {
      Serial.println("writing value");

      nrf52Characteristic.writeValue(outgoing, 8);
      delay(1000);
    }

    // when the central disconnects, print it out:
    Serial.print(F("Disconnected from central: "));
    Serial.println(central.address());
  }

  delay(3000);
}