using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using OpenDataDisc.UI.ViewModels;
using ReactiveUI;
using System; //this fixed some errors, not sure why

namespace OpenDataDisc.UI.Views;

public partial class BluetoothSelectorWindow : ReactiveWindow<BluetoothSelectorViewModel>
{
    public BluetoothSelectorWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode) return;

        this.WhenActivated(action => action(ViewModel!.SelectBluetoothDeviceCommand.Subscribe(Close)));
    }
}