
# OpenDataDisc

An open platform that allows disc golfers to collect statistics about their throws.

> Your throws, your platform, your data.

## Disc Golf + Analytics

I'd love the ablity to track and measure my disc golf throws without spending tons of money.

* Would it be easier to buy one of the currently available products? Probably.
* What's the likelihood this project never completes its goal? Higher than 0.

So why I am doing it? I'm a sucker for starting something new, I'm a software developer, have started playing disc golf and wanted a project to combine those 2.

Will that mix actually produce something usable? No idea.

## Current Project Status

Status: Early Alpha Stage

At this point I just have a microcontroller, some sensors, and (not very much) code.

No timeline on when early prototypes might be available.

## Project Plan

What do I need:

* Some technology/hardware:
  * Microcontroller
  * Accelerometer
  * Battery
  * Connectors/cables
  * Discs to test with
* Some software:
  * Code running on microcontroller to measure everything
  * Code running on host computer to make sense of everything
* Some misc. things:
  * Software skills => I'm a programmer, but not a lot of experience with hardware/microcontroller.
  * Some money => currently have some, no idea how long this might take
  * Willpower, tenacity, drive, etc (How badly do I want it????)

How am I going to accomplish this? I'm going to take this step by step, (no need to rush), document everything, and see where this lands.

* Can I setup my microcontroller with an accelerometer?
  * This gives me a baseline that I can wire up all the basic hardware components correctly
  * Only concerned with measuring angle
* Can I pair my microcontroller with a window's application?
  * I need a host computer to sync my data with.
* Explore battery power options
  * If this is ever going to work, it needs to run on battery.
  * Explore different options of powering microcontroller by battery.
* Construct some type of portable housing for testing
  * I need this thing to be portable to truly measure and verify everything I'm doing.
  * Create something small that I can begin to put through the chops
  * Measure RPMs of this portable device
* Construct some QA (quality assurance) framework/system for verifying my work
  * How do I know my RPMs, speed, angle, wobble are all accurate measurements?
  * Will utilize external hardware to verify this work
* Put in hours of testing/updating code & hardware after testing
  * No idea how long this will take (probably longer than I think, tbh)

That's probably good for now. Will update with more details if I ever get around to it.

## Hardware Components

* Current [esp32 microcontroller](https://www.mouser.com/ProductDetail/Espressif-Systems/ESP32-S3-DevKitM-1-N8?qs=XAiT9M5g4x%2F0QWl%252BQomf2w%3D%3D)
* Current [accelerometer](https://www.mouser.com/ProductDetail/Adafruit/3886?qs=xZ%2FP%252Ba9zWqYWl0i8uQS6xQ%3D%3D)
* Other hardware
  * Our current accelerometer has a [Qwiic connector ](https://www.mouser.com/ProductDetail/Adafruit/4209?qs=PzGy0jfpSMvCXPIwCvMoFg%3D%3D)
  * Usb cable for esp32
  * Random other wires for breadboard

### Changes needed/Notes

* New Accelerometer
  * The Accelerometer listed above only supports up to 2,000 deg/s, which is ~333 RPM. Disc golf throws can go upwards of 1500 RPM. So that won't work long term.
  * I found [this accelerometer](https://www.mouser.com/ProductDetail/Analog-Devices/ADXRS649BBGZ?qs=WIvQP4zGanhEKWMUW9AK8A%3D%3D) that supports up to 20,000 deg/s, which is ~3,333 RPM, more than enough. It's just ~$120ish bucks.
* G forces found in disc golf throws.
  * [Here's a link](https://www.reddit.com/r/discgolf/comments/13fbddc/comment/jjxg0cy/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button) stating a throw can generate up to 175 Gs.

## Things to Explore

https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le/

https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/BluetoothLE