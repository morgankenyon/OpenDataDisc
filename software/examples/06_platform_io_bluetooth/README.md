# CSharp Bluetooth

Up till now, I've been using the python [bleak](https://github.com/hbldh/bleak) library to manage bluetooth connections.

In this experiment, I'm going to try to connect using a C# library called [32Feet](https://github.com/inthehand/32feet). I'm more familiar with C# and I had some problems with bleak. So we'll see how it goes.

## Setting up PlatformIO

Since moving to platformIO, I have yet to do a bluetooth project for my Esp32. So first setting up a new platformIO project.

Setting that up:
* Setup a project
  * `pio project init --board esp32-s3-devkitc-1`
* Ensure the serial monitor baud rate in `main.cpp` and `platformio.ini` is matching.
  * 115200 in my example.
* Blink example is working

On to bluetooth.

## Bluetooth via PlatformIO

Now that I have a project setup, I need to enable bluetooth.

I didn't really have to change much from my code in [04_bluetooth_esp32_to_windows](../04_bluetooth_esp32_to_windows/).

When I was doing that, I kept running into issues with disconnect not working as expected. I read somewhere (linked in the 04 example readme) that this was an issue with Windows.

But when running it today, it worked exactly as I'd expect. So maybe I will be using the bleak library moving forward.

## Final Thoughts

Even though I was expecting to try out C# in this experiment, the python code I had used earlier worked exactly how I expect. Didn't run into the issue I encountered during the 04 example.

I think I'll press forward with using bleak and having client code written in Python. If I encounter any issues, I can comeback to using C#.

## Issues

* I had some problems with `pio` not being able to find the esp32 on COM3. 
  * Generally rerunning the failing command led to it working.
  * No real solution at this point.
* Also the serial monitor in Visual Studio didn't seem to always update to latest code.
  * Had to stop, unplug my esp32, replug, rerun.