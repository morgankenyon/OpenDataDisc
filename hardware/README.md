# OpenDataDisc Hardware

Current status: Attempting to build a putter version. Something that can track low G and low rotation throws.

## Hardware Components

Currently I'm using the following:
* Brains - a [Xiao NRF52 Sense](https://www.seeedstudio.com/Seeed-XIAO-BLE-Sense-nRF52840-p-5253.html)
  * Earlier I used an esp32 microcontroller, but since I didn't need wifi moved to something that fit my requirements and will have longer battery life.
  * This board includes an IMU, USB and LiIon charging circuits built in
* Power - exploring this [110 mAh LiIon battery](https://www.mouser.com/ProductDetail/SparkFun/PRT-13853?qs=wwacUt%252BV97vyGHIz/Bmbzw%3D%3D&countryCode=US&currencyCode=USD), still in the early phases of testing this.
* Casing - 3d printed housing.
  * Currently the files needed to print yourself one are located in [02_putter_housing](./examples/02_putter_housing/).

## DIY Instructions

TBD - right now nothing is integrated. Further instructions will follow as project progresses.

## Examples

The example folder currently contains work that in progress. As hardware designs finalize and are ready for other people to use/build themselves. They will get promoted from the examples folder to this folder.