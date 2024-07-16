
# OpenDataDisc

An open platform that allows disc golfers to collect their throw statistics.

> Your throw, your data, your platform.

## Disc Golf + Analytics

I'd love the ablity to track and measure my disc golf throws without spending tons of money.

* Would it be easier to buy one of the currently available products? Probably.
* What's the likelihood this project never completes its goal? Higher than 0.

So why I am doing it? I'm a sucker for starting something new, I'm a software developer, have started playing disc golf and wanted a project to combine those 2.

Will that mix actually produce something usable? No idea.

## Current Project Phase

Phase: Getting to know hardware

At this point I just have a microcontroller, some sensors, and (not very much) code.

No timeline on when early prototypes might be available.

## Project Phases (Somewhat flexible now)

* Getting to know hardware (current phase)
  * Buying the hardware I think I need.
  * Hooking everything up.
  * Getting data flowing between microcontroller and computer.
  * Getting some basic calculations working
* Putter version
  * Putts have slow RPMs and low Gs
    * These values are much easier to measure and test.
    * Hardware is also cheaper.
    * Will probably be using as many off the shelf parts as possible. (Nothing really custom designed)
  * Create a developer kit that can be used for putters.
  * Need some hardware to confirm the sensor values.
    * Speed gun, high speed camera, etc
  * Most R&D on this project will be based on this platform.
* Alpha Driver Disc
  * To develop something to track high speed throws, will probably need:
    * Custom hardware
    * Beefed up software
    * More expensive testing equipment
* Beta Driver Disc
  * Create a more refined alpha setup
  * Any problems I encountered in the Alpha disc can be changed here
* Driver Disc
  * First proper product release
  * Should be easy for non technical people to setup and use
  * Lots of polish will need to happen before this is ready

## Hardware Components

Currently I'm using the following:
* Brains - a [Xiao NRF52 Sense](https://www.seeedstudio.com/Seeed-XIAO-BLE-Sense-nRF52840-p-5253.html)
  * Earlier I used an esp32 microcontroller, but since I didn't need wifi moved to something that fit my requirements and will have longer battery life.
  * This board includes an IMU, USB and LiIon charging circuits built in
* Power - exploring this [110 mAh LiIon battery](https://www.mouser.com/ProductDetail/SparkFun/PRT-13853?qs=wwacUt%252BV97vyGHIz/Bmbzw%3D%3D&countryCode=US&currencyCode=USD), still in the early phases of testing this.
* Casing - TBD
  * Currently have not tried putting my stuff on a disc