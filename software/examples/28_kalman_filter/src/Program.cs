namespace KalmanCode;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Kalman Filters!");

        DiscreteBayesCode();
    }

    static void GhCode()
    {

        var ghFilter = new GhFilter();
        ghFilter.PredictUsingCalculatedGainRateRunner();

        ghFilter.GhFilterRunner();
    }

    static void DiscreteBayesCode()
    {
        var discreteBayes = new DiscreteBayes();
        discreteBayes.ScaledUpdateRunner();
    }
}
