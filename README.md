
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

## Project Phases (Somewhat flexible now)

* Getting to know hardware (current phase)
  * Buying the hardware I think I need.
  * Hooking everything up.
  * Getting data flowing between microcontroller and computer.
  * Getting some basic calculations working
* Simulating portions of a throw
  * Before creating something that can go on a frisbee
  * Can I simulating calculating RPM, angle, speed in an easier to create configuration
  * Begin to implement procedures/protocols for verifying the measurements I'm taking
    * Speed gun
    * High speed camera ($$$ that I don't know)
    * etc
* Alpha Disc Setup
  * Design something that could be attached to a frisbee
  * Probably still using a development board.
  * Probably will be pretty bulky.
* Beta Disc Setup
  * Create a more refined alpha setup
  * Potentially look into creating a custom PCB to make everything smaller
* Early Release Disc Setup
  * Create something that people could actually try out.
  * Communicate that it will probably still have flaws
  * Might need a technical audience for this phase to help debug issues that come up
* Real thing disc setup
  * First proper product release
  * Should be easy for non technical people to setup and use
  * Lots of polish will need to happen before this is ready

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