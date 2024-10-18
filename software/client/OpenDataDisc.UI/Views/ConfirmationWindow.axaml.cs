using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OpenDataDisc.UI.Models;
using OpenDataDisc.UI.ViewModels;

namespace OpenDataDisc.UI.Views;

public partial class ConfirmationWindow : ReactiveWindow<ConfirmationWindowViewModel>
{
    public ConfirmationWindow()
    {
        InitializeComponent();
    }

    private void OnYesClick(object sender, RoutedEventArgs e)
    {
        this.Close(ConfirmationResult.Yes);
    }

    private void OnNoClick(object sender, RoutedEventArgs e)
    {
        this.Close(ConfirmationResult.No);
    }
}