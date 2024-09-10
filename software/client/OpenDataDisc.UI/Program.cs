using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;

namespace OpenDataDisc.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            //prepare and run app
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LogException(ex);
            Console.WriteLine(ex);
        }
        finally
        {
            Console.WriteLine("Closing application");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    private static void LogException(Exception ex)
    {
        if (ex == null)
            return;

        string logFilePath = "exception.log"; // Define the path for your log file

        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine("--------------------------------------------------");
                writer.WriteLine($"Date: {DateTime.Now}");
                writer.WriteLine($"Exception: {ex.GetType().Name}");
                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"Stack Trace: {ex.StackTrace}");
                writer.WriteLine();
            }
        }
        catch (Exception logEx)
        {
            // Handle any issues with logging itself (e.g., file access issues)
            Console.WriteLine($"Failed to log exception: {logEx.Message}");
        }
    }
}
