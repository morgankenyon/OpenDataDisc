using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using OpenDataDisc.Services.Models;
using OpenDataDisc.UI.Filter;
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
    readonly DataStreamer rollAngleStreamer;
    //readonly DataStreamer pitchAngleStreamer;
    readonly DataStreamer accXStreamer;
    readonly DataStreamer rollTrackerStreamer;
    //readonly DataStreamer magnitudeStreamer;
    readonly DataStreamer gyroYStreamer;
    private DispatcherTimer _addNewDataTimer;
    private DispatcherTimer _updatePlotTimer;
    //private readonly ImuProcessor _imuProcessor = new ImuProcessor();
    private double previousRoll;
    private int rollTracker;
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

        rollAngleStreamer = sensorPlot.Plot.Add.DataStreamer(1000);
        //accXStreamer = sensorPlot.Plot.Add.DataStreamer(1000);
        //rollTrackerStreamer = sensorPlot.Plot.Add.DataStreamer(1000);
        //magnitudeStreamer = sensorPlot.Plot.Add.DataStreamer(1000);
        //gyroYStreamer = sensorPlot.Plot.Add.DataStreamer(300);

        rollAngleStreamer.ViewScrollLeft();
        //accXStreamer.ViewScrollLeft();
        //rollTrackerStreamer.ViewScrollLeft();
        //magnitudeStreamer.ViewScrollLeft();
        //gyroXStreamer.ViewScrollLeft();
        //gyroYStreamer.ViewScrollLeft();

        // disable mouse interaction by default
        sensorPlot.UserInputProcessor.Disable();

        var ekf = new IMUExtendedKalmanFilter(
            gyroInDegrees: true,
            processNoiseScale: 0.001,
            measurementNoise: 0.1
        );

        //how often we should check for new data
        _addNewDataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(5)
        };
        _addNewDataTimer.Tick += (s, e) =>
        {
            ConcurrentQueue<SensorData> queue = MainWindowViewModel.graphingQueue;
            if (queue.TryDequeue(out SensorData sensorData))
            {
                var discConfig = MainWindowViewModel.discConfiguration!;
                var measurement = new ImuMeasurement
                {
                    Timestamp = sensorData.CycleCount,
                    AccelX = sensorData.AccX - (1 - discConfig.AccXOffset),
                    AccelY = sensorData.AccY - (1 - discConfig.AccYOffset),
                    AccelZ = sensorData.AccZ - (1 - discConfig.AccZOffset),
                    GyroX = sensorData.GyroX - discConfig.GyroXOffset,
                    GyroY = sensorData.GyroY - discConfig.GyroYOffset,
                    GyroZ = sensorData.GyroZ - discConfig.GyroZOffset,
                    UptimeMs = sensorData.UptimeMs
                };

                //var timestamp = DateTime.Now;
                //var (accX, accY, accZ) = ReadAccelerometer();
                //var (gyroX, gyroY) = ReadGyroscope();

                var (roll, pitch, lastUpdateTime) = ekf.Update(measurement.UptimeMs,
                    measurement.AccelX,
                    measurement.AccelY,
                    measurement.AccelZ,
                    measurement.GyroX,
                    measurement.GyroY);

                if (roll == previousRoll)
                {
                    rollTracker++;
                }
                else
                {
                    rollTracker = 0;
                    previousRoll = roll;
                }


                //Console.WriteLine($"Roll: {roll * 180 / Math.PI}°, Pitch: {pitch * 180 / Math.PI}°");

                //var newState = _imuProcessor.ProcessMeasurement(measurement);

                double magnitude = Math.Sqrt(measurement.AccelX * measurement.AccelX
                    + measurement.AccelY * measurement.AccelY
                    + measurement.AccelZ * measurement.AccelZ);
                Console.WriteLine($"Acc Magnitude: {magnitude:F3}G");

                rollAngleStreamer.Add(roll);
                //accXStreamer.Add((measurement.AccelX + measurement.AccelY) * 100);
                //rollTrackerStreamer.Add(measurement.Timestamp);
                //magnitudeStreamer.Add(magnitude);
                //gyroXStreamer.Add(sensorData.GyroX);
                //gyroYStreamer.Add(sensorData.GyroY);
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
             Interval = TimeSpan.FromMilliseconds(100)
         };
        _updatePlotTimer.Tick += (s, e) =>
         {
             if (rollAngleStreamer.HasNewData)
             {
                 sensorPlot.Plot.Title($"Processed {rollAngleStreamer.Data.CountTotal:N0} points");
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