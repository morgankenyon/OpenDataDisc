# Platform IO

Up till now, I've been using the Arduino IDE as a development environment.

I'm going to play around with platformIO to see if I like that better.

The cons I keep running into with Arduino IDE:

* Slow compiliation
* Ctrl + V as paste doesn't seem to work
* No code complete.

We'll see if PlatformIO can help me in these regards.

## Setting up PlatformIO

> I'm following the [platformIO documentation](pio project init --board esp32-s3-devkitc-1) for this board.
* Install their `pio` CLI.
* To create a new project, I ran the following command:
  * `pio project init --board esp32-s3-devkitc-1`
  * Took several minutes to complete
  * Now have a platform IO project setup
* Copied over the blink example into `main.cpp`
* Ran the following command:
  * `pio run --target upload`
  * Compliation second time went much faster!!
* My board started blinking!!!
  * First program run successfully!
* Also got the serial monitor working as expected too.

## Moving Forward

I'll probably use PlatformIO for the next couple of examples to see how it does. I might end up redoing some just to test it out. So far compilation seems faster, and it's real C++ (good/bad), it has code complete, everything is based off of a CLI (huge plus).

So we'll see what happens.