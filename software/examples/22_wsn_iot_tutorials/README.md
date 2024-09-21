# WSN & Iot Tutorials

As I'm learning the nrf52840 Dev Kit board, I'm now going to start following this tutorial series: https://www.youtube.com/watch?v=QTtLZIPcYfM&list=PLoKOKJqgqHs9mReagSVnB8FAQBJuRMCEt&index=1&t=14s

It goes through a lot of things about the board and will help me gain a basic understanding.

## Video #1 - Installing

* I downloaded the [nRF Connect for Desktop](https://www.nordicsemi.com/Products/Development-tools/nRF-Connect-for-Desktop/Download?lang=en#infotabs) tool.
  * Currently on v5.0.2.
  * I proceeded to download the Bluetooth Low Energy testing application
  * I also installed the "Toolchain Manager".

## Video #2 - GPIOs, Leds and DeviceTree

Have some examples in `my_second_blinky`, mostly over my head thought.

## Video #3 - Battery Service

https://www.youtube.com/watch?v=CVFbqMMiX2U

Changing over to the bluetooth videos as they are more appropriate for what I want to do.

Code exists in the `ble_battery_service` project.

## Video #4 - Environmental Sensing Service

https://www.youtube.com/watch?v=D9cv3_Z8qwg

Code exists in the `ble_ess` project.

## Video #5 - Custom Service

https://www.youtube.com/watch?v=o-NECS2TN94

Code exists in the `ble_custom_service` project.

## Trying to get notify to work

Code exists in the `ble_test` project.

Links for getting notify to work:

* https://devzone.nordicsemi.com/f/nordic-q-a/94008/custom-notification-characteristic-not-updating
* https://devzone.nordicsemi.com/guides/nrf-connect-sdk-guides/b/software/posts/building-a-ble-application-on-ncs-comparing-and-contrasting-to-softdevice-based-ble-applications#mcetoc_1elnnim8t3

### Issues

I encountered an issue where my Visual studio Code build just reran and reran. I think this was due to my copying projects and paths being pointed to wrong places.

I deleted my build folder and regenerated it via the nRF Connect VS Code Extension and it worked as expected.