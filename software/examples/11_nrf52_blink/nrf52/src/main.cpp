/*
Trying to get blink working for my nrf52Sense
*/
#include "Arduino.h"

bool ledOn = false;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
}

void loop() {
  if (ledOn)
  {
    digitalWrite(LED_BUILTIN, HIGH);
  }
  else
  {
    digitalWrite(LED_BUILTIN, LOW);
  }
  ledOn = !ledOn;

  delay(1000);
}