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