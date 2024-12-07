using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalmanCode
{
    /// <summary>
    /// C# code that follows along with "01-g-h-filter" of the
    /// "Kalman and Bayesian Filters in Python" book
    /// https://github.com/rlabbe/Kalman-and-Bayesian-Filters-in-Python
    /// Hosted locally at http://localhost:8888/notebooks/01-g-h-filter.ipynb
    /// </summary>
    public class GhFilter
    {
        /// <summary>
        /// Runner for PredictUsingGainGuess
        /// </summary>
        public void PredictUsingGainGuessRunner()
        {
            double[] weights = [158.0, 164.2, 160.3, 159.9, 162.1, 164.6,
                169.6, 167.4, 166.4, 171.0, 171.2, 172.6];

            var timeStep = 1.0; //day
            var scaleFactor = 4.0 / 10.0;
            var initialEstimate = 160;

            var (estimates, predictions) = PredictUsingGainGuess(weights, timeStep, scaleFactor, initialEstimate, 1, true);
        }

        /// <summary>
        /// C# version of the python method predict_using_gain_guess(estimated_weight, gain_rate, do_print=False)
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="timeStep"></param>
        /// <param name="scaleFactor"></param>
        /// <param name="estimatedWeight"></param>
        /// <param name="gainRate"></param>
        /// <param name="doPrint"></param>
        /// <returns></returns>
        public (List<double> estimates, List<double> predictions) PredictUsingGainGuess(
            double[] weights,
            double timeStep,
            double scaleFactor,
            double estimatedWeight,
            int gainRate,
            bool doPrint = false)
        {
            var estimates = new List<double>(weights.Length);
            var predictions = new List<double>(weights.Length);

            foreach (var z in weights)
            {
                var previousEstimate = estimatedWeight;
                //Predict new position
                var predictedWeight = estimatedWeight + gainRate * timeStep;

                //Update filter
                estimatedWeight = predictedWeight + scaleFactor * (z - predictedWeight);

                //save and log
                estimates.Add(estimatedWeight);
                predictions.Add(predictedWeight);
                if (doPrint)
                {
                    Console.WriteLine($"previousEstimate: {previousEstimate:f2}, prediction: {predictedWeight:f2}, estimate: {estimatedWeight:f2}");
                }
            }

            return (estimates, predictions);
        }

        public void PredictUsingCalculatedGainRateRunner()
        {
            double[] weights = [158.0, 164.2, 160.3, 159.9, 162.1, 164.6,
                169.6, 167.4, 166.4, 171.0, 171.2, 172.6];
            var weight = 160;
            var gainRate = -1.0;
            var timeStep = 1.0;
            var weightScale = 4.0 / 10;
            var gainScale = 1.0 / 3;

            PredictUsingCalculatedGainRate(weights, timeStep, weight, gainRate, gainScale, weightScale, true);
        }

        public (List<double> estimates, List<double> predictions) PredictUsingCalculatedGainRate(
            double[] weights,
            double timeStep,
            double weight,
            double gainRate,
            double gainScale,
            double weightScale,
            bool doPrint = false)
        {
            var estimates = new List<double>(weights.Length);
            var predictions = new List<double>(weights.Length);
            foreach (var z in weights)
            {
                var previousEstimate = weight;
                //prediction step
                weight = weight + gainRate * timeStep;
                gainRate = gainRate; //why is this in here?
                predictions.Add(weight);
                var predictedWeight = weight;

                //update step
                var residual = z - weight;

                gainRate = gainRate + gainScale * (residual / timeStep);
                weight = weight + weightScale * residual;
                
                estimates.Add(weight);
                var estimatedWeight = weight;
                if (doPrint)
                {
                    Console.WriteLine($"previousEstimate: {previousEstimate:f2}, measurement: {z:f2}, prediction: {predictedWeight:f2}, estimate: {estimatedWeight:f2}, gainRate: {gainRate:f2}");
                }
            }

            return (estimates, predictions);
        }
    }
}
