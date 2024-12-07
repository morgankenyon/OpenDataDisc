namespace KalmanCode;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Kalman Filters!");

        var ghFilter = new GhFilter();
        ghFilter.PredictUsingCalculatedGainRateRunner();
    }
}
