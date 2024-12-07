namespace KalmanCode;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Kalman Filters!");

        GhCode();
    }

    static void GhCode()
    {

        var ghFilter = new GhFilter();
        ghFilter.PredictUsingCalculatedGainRateRunner();

        ghFilter.GhFilterRunner();
    }
}
