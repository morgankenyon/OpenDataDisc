using Avalonia.Controls;
using OpenDataDisc.UI.ViewModels;
using OpenDataDisc.UI;
using ReactiveUI;
using System.Threading.Tasks;
using OpenDataDisc.ViewModels;
using Avalonia.ReactiveUI;

namespace OpenDataDisc.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(action =>
            action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(InteractionContext<MusicStoreViewModel,
                                        AlbumViewModel?> interaction)
    {
        var dialog = new MusicStoreWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<AlbumViewModel?>(this);
        interaction.SetOutput(result);
    }
}