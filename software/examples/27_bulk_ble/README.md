# Bulk Bluetooth Send

Now that I can read all the necessary data points, I want to work on sending larger bluetooth messages.

If I send the measurements 1 by 1, I can get roughly 60 msg/sec. I would like to closer to 250 msg/sec.

So my thought is storing the last 1000 messages, then whenever I "detect" a throw, send all 1000 messages over at once.

## My goals

1. Send many data points over one bluetooth message.

## Issues
