using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.Models;
using OpenDataDisc.UI.ViewModels;
using ReactiveUI;
using ScottPlot.Avalonia;
using System.Threading.Tasks;

namespace OpenDataDisc.UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(action =>
            action(ViewModel!.ShowBluetoothDialog.RegisterHandler(DoShowBluetoothSelectorAsync)));
        this.WhenActivated(action =>
            action(ViewModel!.ShowConfirmationDialog.RegisterHandler(DoShowConfirmationDialogAsync)));
        this.WhenActivated(action =>
            action(ViewModel!.ShowConfigurationDialog.RegisterHandler(DoShowConfigurationDialogAsync)));



        double[] dataX = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        double[] dataY = { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 };

        AvaPlot avaPlot1 = this.Find<AvaPlot>("AvaPlot1");
        avaPlot1.Plot.Add.Scatter(dataX, dataY);
        avaPlot1.Refresh();
    }

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

    private async Task DoShowConfigurationDialogAsync(
        InteractionContext<ConfigurationWindowViewModel, DiscConfigurationData> interaction)
    {
        var dialog = new ConfigurationWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<DiscConfigurationData>(this);
        interaction.SetOutput(result);
    }
}