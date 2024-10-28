using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.ViewModels;
using System;

namespace OpenDataDisc.UI.Views;

public partial class ConfigurationWindow : ReactiveWindow<ConfigurationWindowViewModel>
{
    public ConfigurationWindow()
    {
        InitializeComponent();
    }

    private void OnYesClick(object sender, RoutedEventArgs e)
    {
        this.Close(new DiscConfigurationData(
            "",
            DateTime.Now.Ticks,
            23.2,
            29.2,
            29.2));
    }

    private void OnNoClick(object sender, RoutedEventArgs e)
    {
        this.Close(new DiscConfigurationData(
            "",
            DateTime.Now.Ticks,
            23.2,
            29.2,
            29.2));
    }
}