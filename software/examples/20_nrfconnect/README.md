# Using NRF Connect

Up till now I've been using the Arduino based libraries. Both through Arduino IDE and PlatformIO.

My first measurement showed that I can get roughly 50 bluetooth msg/sec delivered from my nrf52 to my computer. That is not high enough for what I'm looking to measure.

So I'm now looking about using Nordic Semiconductor's (the maker of NRF 52840) proper SDK.

## Installation

> I'm currently using windows 11 device

* I followed this video:
  * https://www.youtube.com/watch?v=EAJdOqsL9m8
  * Downloaded the command line tools: https://www.nordicsemi.com/Products/Development-tools/nRF-Command-Line-Tools/Download
    * That included the SEEGER debugging tools
  * Dowload the nRF Connection for VS Code Extensions:
  ![visual studio code install](image.png)
    * Install the tool chain (I downloaded 2.7.0)
    * Install SDK (through "Manage SDKs" option)
