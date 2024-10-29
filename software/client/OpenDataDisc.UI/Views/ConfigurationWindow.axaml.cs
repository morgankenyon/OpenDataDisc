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

    private void NextStep(object sender, RoutedEventArgs e)
    {
        var viewModel = this.DataContext as ConfigurationWindowViewModel;

        if (viewModel != null)
        {
            viewModel.AdvanceToNextStep();
        }
    }

    private void CloseModal(object sender, RoutedEventArgs e)
    {
        var viewModel = this.DataContext as ConfigurationWindowViewModel;

        if (viewModel != null)
        {
            this.Close(viewModel.DiscConfiguration);
        }
    }
}