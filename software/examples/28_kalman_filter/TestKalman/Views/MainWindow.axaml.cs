using Avalonia.Controls;
using Avalonia.Threading;
using ScottPlot.Plottables;
using System;
using System.Linq;

namespace TestKalman.Views;

public partial class MainWindow : Window
{
    readonly DataStreamer dataStreamer;
    readonly ScottPlot.DataGenerators.RandomWalker walker = new(0);
    private DispatcherTimer _addNewDataTimer;
    private DispatcherTimer _updatePlotTimer;

    public MainWindow()
    {
        InitializeComponent();

        double[] dataX = new double[] { 1, 2, 3, 4, 5 };
        double[] dataY = new double[] { 1, 4, 9, 16, 25 };

        // Clear any existing plots
        AvaPlot1.Plot.Clear();

        dataStreamer = AvaPlot1.Plot.Add.DataStreamer(300);
        dataStreamer.ViewScrollLeft();
        //how often we should check for new data
        _addNewDataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        _addNewDataTimer.Tick += (s, e) =>
        {
            int count = 5;
            // add new sample data
            var nextValue = walker.Next(count);
            dataStreamer.AddRange(nextValue);
            // slide marker to the left
            AvaPlot1.Plot.GetPlottables<Marker>()
                .ToList()
                .ForEach(m => m.X -= count);
            // remove off-screen marks
            AvaPlot1.Plot.GetPlottables<Marker>()
                .Where(m => m.X < 0)
                .ToList()
                .ForEach(m => AvaPlot1.Plot.Remove(m));
        };
        _addNewDataTimer.Start();

        //how often we refresh the plot
        _updatePlotTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _updatePlotTimer.Tick += (s, e) =>
        {
            if (dataStreamer.HasNewData)
            {
                AvaPlot1.Plot.Title($"Processed {dataStreamer.Data.CountTotal:N0} points");
                AvaPlot1.Refresh();
            }
        };
        _updatePlotTimer.Start();

        // Add a scatter plot
        //AvaPlot1.Plot.Add.Scatter(dataX, dataY);

        //// Optionally customize the plot
        //AvaPlot1.Plot.Title("My Plot");
        //AvaPlot1.Plot.XLabel("X Axis");
        //AvaPlot1.Plot.YLabel("Y Axis");

        //// Refresh the plot to show changes
        //AvaPlot1.Refresh();
    }
}