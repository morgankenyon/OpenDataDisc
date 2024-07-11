# Setting Up My NRF52Sense

I realized that my esp32 was a little overkill since I didn't need to support wifi.

I am trying out an NRF52 Sense. It only supports bluetooth (no wifi), so should offer better battery life.

## Creating a new PlatformIO Project

Based on [this PR](https://github.com/platformio/platform-nordicnrf52/pull/151) PlatformIO doesn't properly support NRF52Sense yet. To get a PlatformIO project working with this board I did the following:


* Run `pio project init --board uno` to create a basic platformIO project
* Replace the `platformio.ini` file with:
```text
[env:xiaoblesense_arduinocore_mbed]
platform = https://github.com/maxgerhardt/platform-nordicnrf52
framework = arduino
board = xiaoblesense
```
* Create a `src/main.cpp` and put in a simple blink program:
```c++
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
```
* Run `pio run --target upload`
* Board should blink

## Issues I Ran Into

* The basic PlatformIO project I created didn't have a `main.cpp` automatically created. I got the following error:
```txt
c:/users/morga/.platformio/packages/toolchain-gccarmnoneeabi/bin/../lib/gcc/arm-none-eabi/8.2.1/../../../../arm-none-eabi/bin/ld.exe: .pio\build\xiaoblesense_arduinocore_mbed\libFrameworkArduino.a(main.cpp.o): in function `main':
main.cpp:(.text.startup.main+0x1e): undefined reference to `setup'
c:/users/morga/.platformio/packages/toolchain-gccarmnoneeabi/bin/../lib/gcc/arm-none-eabi/8.2.1/../../../../arm-none-eabi/bin/ld.exe: main.cpp:(.text.startup.main+0x22): undefined reference to `loop'
collect2.exe: error: ld returned 1 exit status
*** [.pio\build\xiaoblesense_arduinocore_mbed\firmware.elf] Error 1
```

  * To fix it, I referenced the [seeed wiki page](https://wiki.seeedstudio.com/XIAO_BLE/#playing-with-the-built-in-3-in-one-led) where it gave clues about how to get blink working.


* Setting up the platform IO project for this board is still unsupported, so I had to reference the [github PR](https://github.com/platformio/platform-nordicnrf52/pull/151) to get it setup