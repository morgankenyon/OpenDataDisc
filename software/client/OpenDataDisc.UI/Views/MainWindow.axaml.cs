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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDataDisc.UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    readonly DataStreamer accXStreamer;
    readonly DataStreamer accYStreamer;
    readonly DataStreamer gyroXStreamer;
    readonly DataStreamer gyroYStreamer;
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

        AvaPlot sensorPlot = this.Find<AvaPlot>("SensorPlot");

        accXStreamer = sensorPlot.Plot.Add.DataStreamer(300);
        accYStreamer = sensorPlot.Plot.Add.DataStreamer(300);
        gyroXStreamer = sensorPlot.Plot.Add.DataStreamer(300);
        gyroYStreamer = sensorPlot.Plot.Add.DataStreamer(300);

        accXStreamer.ViewScrollLeft();
        accYStreamer.ViewScrollLeft();
        gyroXStreamer.ViewScrollLeft();
        gyroYStreamer.ViewScrollLeft();

        // disable mouse interaction by default
        sensorPlot.UserInputProcessor.Disable();

        //how often we should check for new data
        _addNewDataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        _addNewDataTimer.Tick += (s, e) =>
        {
            ConcurrentQueue<SensorData> queue = MainWindowViewModel.graphingQueue;
            if (queue.TryDequeue(out SensorData result))
            {

                accXStreamer.Add(result.AccX);
                //accYStreamer.Add(result.AccY);
                //gyroXStreamer.Add(result.GyroX);
                //gyroYStreamer.Add(result.GyroY);

                // slide marker to the left
                sensorPlot.Plot.GetPlottables<Marker>()
                    .ToList()
                    .ForEach(m => m.X -= 1);

                // remove off-screen marks
                sensorPlot.Plot.GetPlottables<Marker>()
                    .Where(m => m.X < 0)
                    .ToList()
                    .ForEach(m => sensorPlot.Plot.Remove(m));
            }
        };
        _addNewDataTimer.Start();

        //how often we refresh the plot
        _updatePlotTimer = new DispatcherTimer
         {
             Interval = TimeSpan.FromMilliseconds(50)
         };
        _updatePlotTimer.Tick += (s, e) =>
         {
             if (accXStreamer.HasNewData)
             {
                 sensorPlot.Plot.Title($"Processed {accXStreamer.Data.CountTotal:N0} points");
                 sensorPlot.Refresh();
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