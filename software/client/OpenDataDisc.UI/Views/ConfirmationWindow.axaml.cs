using Avalonia.Controls;
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

        //if (Design.IsDesignMode) return;

        //this.WhenActivated(action => 
        //    action(ViewModel!.DisconnectBluetoothCommand.Subscribe(Close)));
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