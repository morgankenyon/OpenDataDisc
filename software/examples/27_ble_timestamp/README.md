# Sending Timestamps With Bluetooth Messages

Now that I can send over all my data, I need a time value to go along with it.

My first thought was using the number of milliseconds since the epoch.

But since Zephyr is a RTOS, there's no absolute time mechanism. So I decided to get the cycle numbers as a psuedo time marker.

So I'm using this function:

```c
uint32_t get_cycle_count()
{
    return k_cycle_get_32();
}
```

To get the cycle count as a 32 unsigned int. Since it's a 32 bit integer, there's a chance it could rollover to 0 and cause problems with calculations.

Based on my calculations, it would take 40ish hours of consistent running to get from 0 to 4 billion clock cycles. Since the odds of that is pretty small, I don't think I have to worry about that.

## My goals

1. Send over a timestamp along with the sensor values
