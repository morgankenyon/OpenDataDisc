# OpenDataDisc Software

This folder holds all relevant software for the OpenDataDisc project. Software is split into 2 different sub projects:

* client - .NET based project that runs on a Windows PC. Receives telemetry about the disc from the microcontroller and displays it to the user.
* microcontroller - attached to the disc and is responsible for gathering data about the discs acceleration and movement. Sends that information to the client.

## Current Architectural Guidelines

* Have microcontroller code be as simple as possible.
  * Professionally I'm a .NET developer, I'm more comfortable programming in C#. 
  * I don't want the microcontroller code to get too complex.
* Microcontroller sends raw data to the client
  * Currently the microcontroller does not do any calculations of it's own.
  * It sends pulls in data from sensors and sends to client.
* Client handles all complex calculations
  * Any calculations needed for throws happens on the client.

## How to Run

TBD

Right now the limiting factor in trying this yourself is hardware. I'm still building out a putter version, so until that's at a point where people can copy it there's really no sense it getting this running.

## Examples

In the examples folder are numerous attempts I had to get certain portions of this project running individually. (Can I connect to bluetooth, can I read accelerometer values, can I create a simple Avalonia application). There's not much value in them now.

Our current `/client` and `/microcontroller` code started as examples and were promoted as they became ready.