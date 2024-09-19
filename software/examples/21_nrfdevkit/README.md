# nRF 52840 Dev Kit

I am trying to get better performance out of my xiao sense. Right now only sending 50 msg/sec, and very limited in terms of bytes (23 bytes per message I believe).

That has lead me to looking outside of the Arduino/PlatformIO ecosystem. I'm experimenting with NRFConnect from Nordic. Since most of their examples target their dev board, I purchased a [nRF52840-DK](https://www.mouser.com/ProductDetail/Nordic-Semiconductor/nRF52840-DK?qs=F5EMLAvA7IA76ZLjlwrwMw%3D%3D).

And will be trying to learn it, Zephyr OS and others to hopefully improve my performance.

I imagine I'll have to do more things from scratch, probably writing accelerometer libraries?? Learn C properly. So this should be fun.

## Troubleshooting

* Make sure you turn your dev kit on via the power button. 
  * I plugged it in expecting it to work like my other boards, but it wouldn't turn on.
  * finally remembered about the power switch.
* Couldn't flash device with error: "ERROR: No debuggers were discovered."
  * Other error: "FATAL ERROR: command exited with status 41: nrfjprog"
  * Seems to be an issue with the Seeger version that came with nrfConnect (7.94)
  * https://devzone.nordicsemi.com/f/nordic-q-a/108416/unable-to-flash-blinky
  * I downloaded to version 7.88j and it detected it as expected.
* New error: ERROR: The operation attempted is unavailable due to readback protection in ERROR: your device. Please use --recover to unlock the device.
  * I ran this command from my terminal and it worked as expected: `nrfjprog --recover`
  * https://devzone.nordicsemi.com/f/nordic-q-a/55853/invalid-nrf52833-read-back-protection-recovery-not-working-with-nrfjprog