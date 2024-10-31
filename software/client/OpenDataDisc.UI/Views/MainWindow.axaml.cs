using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.Models;
using OpenDataDisc.UI.ViewModels;
using ReactiveUI;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDataDisc.UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    readonly ScottPlot.Plottables.DataStreamer Streamer1;
    readonly ScottPlot.DataGenerators.RandomWalker Walker1 = new(0);
    readonly ScottPlot.Plottables.VerticalLine VLine;
    private DispatcherTimer _addNewDataTimer;
    private DispatcherTimer _updatePlotTimer;
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
        //avaPlot1.Plot.Add.Scatter(dataX, dataY);
        //avaPlot1.Refresh();

        var Streamer1 = avaPlot1.Plot.Add.DataStreamer(1000);
        VLine = avaPlot1.Plot.Add.VerticalLine(0, 2, ScottPlot.Colors.Red);

        // disable mouse interaction by default
        avaPlot1.UserInputProcessor.Disable();

        // only show marker button in scroll mode
        //btnMark.Visible = false;
        _addNewDataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        _addNewDataTimer.Tick += (s, e) =>
        {
            int count = 5;

            // add new sample data
            var nextValue = Walker1.Next(count);
            Streamer1.AddRange(nextValue);

            // slide marker to the left
            avaPlot1.Plot.GetPlottables<Marker>()
                .ToList()
                .ForEach(m => m.X -= count);

            // remove off-screen marks
            avaPlot1.Plot.GetPlottables<Marker>()
                .Where(m => m.X < 0)
                .ToList()
                .ForEach(m => avaPlot1.Plot.Remove(m));
        };
        _addNewDataTimer.Start();

        _updatePlotTimer = new DispatcherTimer
         {
             Interval = TimeSpan.FromMilliseconds(50)
         };
        _updatePlotTimer.Tick += (s, e) =>
         {
             if (Streamer1.HasNewData)
             {
                 avaPlot1.Plot.Title($"Processed {Streamer1.Data.CountTotal:N0} points");
                 VLine.IsVisible = Streamer1.Renderer is ScottPlot.DataViews.Wipe;
                 VLine.Position = Streamer1.Data.NextIndex * Streamer1.Data.SamplePeriod + Streamer1.Data.OffsetX;
                 avaPlot1.Refresh();
             }
         };
        _updatePlotTimer.Start();
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