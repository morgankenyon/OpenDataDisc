# Avalonia with Bluetooth

My first attempt to get bluetooth working with Avalonia. I'm basically going to be combining code from example 15 and 16 to see if I can get everything working together.

* Uploading to nrf52
  * `pio run --target upload`

This example now allows you to select a bluetooth device, subscribe to it, then display messages on the home screen.

## Resources

* Example 15
* Example 16
* Avalonia Music Store Tutorial - https://docs.avaloniaui.net/docs/tutorials/music-store-app/creating-the-project
* "AVLN:0004	Unable to resolve property or method of name '' on type ''"
  * `public ObservableCollection<string> DiscMessages = new();`
  * To
  * `public ObservableCollection<string> DiscMessages { get; } = new();`
