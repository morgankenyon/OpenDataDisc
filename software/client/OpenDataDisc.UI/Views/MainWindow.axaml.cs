using Avalonia.ReactiveUI;
using OpenDataDisc.UI.Models;
using OpenDataDisc.UI.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;

namespace OpenDataDisc.UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        //this.WhenActivated(action =>
        //    action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));

        this.WhenActivated(action =>
            action(ViewModel!.ShowBluetoothDialog.RegisterHandler(DoShowBluetoothSelectorAsync)));
        this.WhenActivated(action =>
            action(ViewModel!.ShowConfirmationDialog.RegisterHandler(DoShowConfirmationDialogAsync)));
    }

    //private async Task DoShowDialogAsync(InteractionContext<MusicStoreViewModel,
    //                                    AlbumViewModel?> interaction)
    //{
    //    var dialog = new MusicStoreWindow();
    //    dialog.DataContext = interaction.Input;

    //    var result = await dialog.ShowDialog<AlbumViewModel?>(this);
    //    interaction.SetOutput(result);
    //}

    private async Task DoShowBluetoothSelectorAsync(
        InteractionContext<BluetoothSelectorViewModel, SelectedDeviceViewModel?> interaction)
    {
        var dialog = new BluetoothSelectorWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<SelectedDeviceViewModel?>(this);
        interaction.SetOutput(result);

    }

    private async Task DoShowConfirmationDialogAsync(
        InteractionContext<ConfirmationWindowViewModel, ConfirmationResult> interaction)
    {
        var dialog = new ConfirmationWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<ConfirmationResult>(this);
        interaction.SetOutput(result);
    }
}