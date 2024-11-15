using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using TestKalman.Filters;

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

        var entryAssembly = Assembly.GetEntryAssembly().GetName().Name;
        var uri = new Uri($"avares://{entryAssembly}/Assets/imuDataSimpleXAxisRock.json");
        using var stream = AssetLoader.Open(uri);

        // For text files:
        using var reader = new StreamReader(stream);
        string content = reader.ReadToEnd();

        var sensorData = JsonSerializer.Deserialize<List<SensorData>>(content);
        var imuData = TransformSensorData(sensorData);
        
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
    }

    private List<IMUData> TransformSensorData(List<SensorData>? sensorDataList)
    {
        var dataList = new List<IMUData>();

        if (sensorDataList == null)
            return dataList;

        var sensorData = sensorDataList.First();
        foreach (var row in sensorData.rows)
        {
            if (row.Count == 8)
            {
                dataList.Add(new IMUData
                {
                    Timestamp = uint.Parse(row[1]),
                    Ax = double.Parse(row[2]),
                    Ay = double.Parse(row[3]),
                    Az = double.Parse(row[4]),
                    Gx = double.Parse(row[5]),
                    Gy = double.Parse(row[6]),
                    Gz = double.Parse(row[7])
                });
            }
        }

        return dataList;
    }
}