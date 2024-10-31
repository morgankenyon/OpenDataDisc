# Commonly Occurred Errors

## Avalonia Errors

### Statis resource not found

Encountered the following exception:
```
Exception: KeyNotFoundException
Message: Static resource 'm:StateToVisibilityConverter' not found.
Stack Trace:    at Avalonia.Markup.Xaml.MarkupExtensions.StaticResourceExtension.ProvideValue(IServiceProvider serviceProvider)
   at OpenDataDisc.UI.Views.MainWindow.!XamlIlPopulate(IServiceProvider, MainWindow) in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\Views/MainWindow.axaml:line 41
   at OpenDataDisc.UI.Views.MainWindow.!XamlIlPopulateTrampoline(MainWindow)
   at OpenDataDisc.UI.Views.MainWindow.InitializeComponent(Boolean loadXaml, Boolean attachDevTools) in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\obj\Debug\net8.0-windows10.0.22621.0\Avalonia.Generators\Avalonia.Generators.NameGenerator.AvaloniaNameSourceGenerator\OpenDataDisc.UI.Views.MainWindow.g.cs:line 23
   at OpenDataDisc.UI.Views.MainWindow..ctor() in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\Views\MainWindow.axaml.cs:line 12
   at OpenDataDisc.UI.App.OnFrameworkInitializationCompleted() in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\App.axaml.cs:line 49
   at Avalonia.AppBuilder.SetupUnsafe()
   at Avalonia.AppBuilder.Setup()
   at Avalonia.AppBuilder.SetupWithLifetime(IApplicationLifetime lifetime)
   at Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)
   at OpenDataDisc.UI.Program.Main(String[] args) in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\Program.cs:line 19
```

I wasn't referencing my converter properly. I fixed this by introducing a `Window.Resources` section to define and import (might not be the proper term) this namespace/object.

```xml
	<Window.Resources>
		<m:StateToVisibilityConverter x:Key="StateToVisibilityConverter" />
	</Window.Resources>
```

## Zephyr Bluetooth

`C:/ncs/v2.7.0/nrf/include/bluetooth/scan.h:223:36: error: 'CONFIG_BT_SCAN_UUID_CNT' undeclared here (not in a function)`

Couldn't fix this issue. Ended up removing the extra code I used for the connect/disconnect bluetooth callbacks.

## Issue Writing Message to Server

In trying to get configuration to work. It's something the client needs to inform the server that is wants to do. I wanted to Write a message to the Server.

When I tried I got the following error:
```
   at InTheHand.Bluetooth.GattCharacteristic.<PlatformWriteValue>d__36.MoveNext()
   at OpenDataDisc.UI.ViewModels.MainWindowViewModel.<ListenToDevice>d__45.MoveNext() in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\ViewModels\MainWindowViewModel.cs:line 267
   at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__128_0(Object state)
   at Avalonia.Threading.SendOrPostCallbackDispatcherOperation.InvokeCore()
   at Avalonia.Threading.DispatcherOperation.Execute()
   at Avalonia.Threading.Dispatcher.ExecuteJob(DispatcherOperation job)
   at Avalonia.Threading.Dispatcher.ExecuteJobsCore(Boolean fromExplicitBackgroundProcessingCallback)
   at Avalonia.Threading.Dispatcher.Signaled()
   at Avalonia.Win32.Win32Platform.WndProc(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam)
   at Avalonia.Win32.Interop.UnmanagedMethods.DispatchMessage(MSG& lpmsg)
   at Avalonia.Win32.Win32DispatcherImpl.RunLoop(CancellationToken cancellationToken)
   at Avalonia.Threading.DispatcherFrame.Run(IControlledDispatcherImpl impl)
   at Avalonia.Threading.Dispatcher.PushFrame(DispatcherFrame frame)
   at Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken)
   at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.Start(String[] args)
   at Avalonia.ClassicDesktopStyleApplicationLifetimeExtensions.StartWithClassicDesktopLifetime(AppBuilder builder, String[] args, Action`1 lifetimeBuilder)
   at OpenDataDisc.UI.Program.Main(String[] args) in C:\dev\OpenDataDisc\software\client\OpenDataDisc.UI\Program.cs:line 19

```
If I catch the exception:

```csharp
byte[] configureMessage = Encoding.UTF8.GetBytes("configure\n");
try
{

   await chars.WriteValueWithResponseAsync(configureMessage);
}
catch (System.Runtime.InteropServices.COMException comException)
{
   configureMessage = Encoding.ASCII.GetBytes("hello");
}
```

The ComException has an empty error message the following error code:`0x80650003`

Stackoverflow with same error code: https://stackoverflow.com/questions/38804878/uwp-system-runtime-interop-comexception-on-debug-but-system-exception-on-release
Something to try: https://stackoverflow.com/questions/48973287/failed-to-subscribe-to-notification-characteristic-in-bluetooth-low-energy

It ended up being a permissions issue. I was configuring bluetooth in zephyr as such:

```c
BT_GATT_SERVICE_DEFINE(custom_srv,
	BT_GATT_PRIMARY_SERVICE(ODD_SERVICE),
	BT_GATT_CHARACTERISTIC(ODD_SENSOR_CHRC, 
        BT_GATT_CHRC_NOTIFY | BT_GATT_CHRC_WRITE | BT_GATT_CHRC_WRITE_WITHOUT_RESP,
        BT_GATT_PERM_NONE,       //this right here
        NULL,
        on_write,
        NULL),
	BT_GATT_CCC(NULL, BT_GATT_PERM_READ | BT_GATT_PERM_WRITE),
);
```

I changed "BT_GATT_PERM_NONE" => "BT_GATT_PERM_WRITE" and my server was able to receive the message from the client.