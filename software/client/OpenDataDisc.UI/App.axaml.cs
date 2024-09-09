using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OpenDataDisc.Services;
using OpenDataDisc.Services.Interfaces;
using OpenDataDisc.UI.ViewModels;
using OpenDataDisc.UI.Views;
using System;
using System.Threading.Tasks;

namespace OpenDataDisc.UI;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; }

    public App()
    {
        var serviceCollection = new ServiceCollection();

        ConfigureServices(serviceCollection);

        Services = serviceCollection.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenDataDiscServices();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var dataSchemaService = Services.GetRequiredService<IDataSchemaService>();

        //TODO: need error handling logic here
        dataSchemaService.MigrateSchemaToLatest().GetAwaiter().GetResult();

        var sensorService = Services.GetRequiredService<ISensorService>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(sensorService),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}