# OpenDataDisc

A github repo to document my exploration into creating an open source data tracking disc.

## Disc Golf + Analytics

I'd love the ablity to track and measure my disc golf throws without spending tons of money.

Would it be easier to buy one of the currently available products? Probably.

What's the likelihood this project never goes anywhere? Higher than 0.

So why I am doing it? I'm a sucker for starting something new, I'm a software developer and have started playing disc golf recently.

## Project Plan

As of right now, this is just an idea. There's no disc, no tech, I've put no work into it. But, there is a semblance of a plan.

What do I need:

* Some technology/hardware:
  * Microcontroller
  * Accelerometer
  * Battery
  * Connectors/cables
  * Discs to test with
* Some software:
  * Code running on microcontroller to measure everything
  * Code running on host computer to make sense of everything
* Some misc. things:
  * Software skills => I'm a programmer, but not a lot of experience with hardware/microcontroller.
  * Some money => currently have some, no idea how long this might take
  * Willpower, tenacity, drive, etc (How badly do I want it????)

How am I going to accomplish this? I'm going to take this step by step, (no need to rush), document everything, and see where this lands.

* Can I setup my microcontroller with an accelerometer?
  * This gives me a baseline that I can wire up all the basic hardware components correctly
  * Only concerned with measuring angle
* Can I pair my microcontroller with a window's application?
  * I need a host computer to sync my data with.
* Explore battery power options
  * If this is ever going to work, it needs to run on battery.
  * Explore different options of powering microcontroller by battery.
* Construct some type of portable housing for testing
  * I need this thing to be portable to truly measure and verify everything I'm doing.
  * Create something small that I can begin to put through the chops
  * Measure RPMs of this portable device
* Construct some QA (quality assurance) framework/system for verifying my work
  * How do I know my RPMs, speed, angle, wobble are all accurate measurements?
  * Will utilize external hardware to verify this work
* Put in hours of testing/updating code & hardware after testing
  * No idea how long this will take (probably longer than I think, tbh)

That's probably good for now.