using System.Diagnostics.CodeAnalysis;

namespace KalmanCode
{
    /// <summary>
    /// C# code that follows along with "02-Discrete-Bayes" chapter of the
    /// "Kalman and Bayesian Filters in Python" book
    /// https://github.com/rlabbe/Kalman-and-Bayesian-Filters-in-Python
    /// Hosted locally at http://localhost:8888/notebooks/02-Discrete-Bayes.ipynb
    /// </summary>
    public class DiscreteBayes
    {
        public void UpdateBeliefRunner()
        {
            int[] hallway = [1, 1, 0, 0, 0, 0, 0, 0, 1, 0];
            double[] belief = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1];
            var reading = 1;

            UpdateBelief(hallway, belief, reading, 3.0);

            foreach (var val in belief)
            {
                Console.WriteLine($"Belief: {val:f2}");
            }

            var sum = belief.Sum(m => m);
            Console.WriteLine($"Sum: {sum:f2}");

            var scaledBelief = new double[10];
            for(int i = 0; i < belief.Length; i++)
            {
                scaledBelief[i] = belief[i] / sum;
            }

            foreach (var val in scaledBelief)
            {
                Console.WriteLine($"ScaleBelief: {val:f3}");
            }
        }

        /// <summary>
        /// C# version of the python method update_belief(hall, belief, z, correct_scale):
        /// </summary>
        /// <param name="hallway"></param>
        /// <param name="belief"></param>
        /// <param name="z"></param>
        /// <param name="correctScale"></param>
        public void UpdateBelief(
            int[] hallway,
            double[] belief,
            int z,
            double correctScale)
        {
            for(int i = 0; i < hallway.Length; i++)
            {
                var val = hallway[i];

                if (val == z)
                {
                    belief[i] *= correctScale;
                }
            }
        }

        /// <summary>
        /// Normalizes the array to a probability distribution
        /// </summary>
        /// <param name="values"></param>
        public void Normalize(double[] values)
        {
            var sum = values.Sum(m => m);

            for(int i = 0; i < values.Length; i++)
            {
                values[i] = values[i] / sum;
            }
        }

        public void ScaledUpdateRunner()
        {
            int[] hallway = [1, 1, 0, 0, 0, 0, 0, 0, 1, 0];
            double[] belief = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1];
            var reading = 1;

            ScaledUpdate(hallway, belief, reading, 0.75);

            var sum = belief.Sum(m => m);
            Console.WriteLine($"sum = {sum}");
            Console.WriteLine($"probability of door = {belief[0]}");
            Console.WriteLine($"probability of wall = {belief[2]}");
        }

        /// <summary>
        /// C# version of the python method scaled_update(hall, belief, z, z_prob)
        /// </summary>
        /// <param name="hallway"></param>
        /// <param name="belief"></param>
        /// <param name="z"></param>
        /// <param name="z_prob"></param>
        public void ScaledUpdate(
            int[] hallway,
            double[] belief,
            int z,
            double z_prob)
        {
            var scale = z_prob / (1.0 - z_prob);
            for (int i = 0; i < hallway.Length; i++)
            {
                if (hallway[i] == z)
                {
                    belief[i] = belief[i] * scale;
                }
            }

            Normalize(belief);
        }

        public void ScaledUpdateRunner2()
        {
            int[] hallway = [1, 1, 0, 0, 0, 0, 0, 0, 1, 0];
            double[] belief = [0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1];
            var reading = 1;

            ScaledUpdate2(hallway, belief, reading, 0.75);

            var sum = belief.Sum(m => m);
            Console.WriteLine($"sum = {sum}");
            Console.WriteLine($"probability of door = {belief[0]}");
            Console.WriteLine($"probability of wall = {belief[2]}");
        }

        /// <summary>
        /// Same method as above, but comment indicating that the middle step in computing the likelihood array
        /// </summary>
        /// <param name="hallway"></param>
        /// <param name="belief"></param>
        /// <param name="z"></param>
        /// <param name="z_prob"></param>
        public void ScaledUpdate2(
            int[] hallway,
            double[] belief,
            int z,
            double z_prob)
        {
            var scale = z_prob / (1.0 - z_prob);
            //computer likelihood
            for (int i = 0; i < hallway.Length; i++)
            {
                if (hallway[i] == z)
                {
                    belief[i] = belief[i] * scale;
                }
            }

            Normalize(belief);
        }

        /// <summary>
        /// C# version of python method update(likelihood, prior)
        /// </summary>
        /// <param name="likelihood"></param>
        /// <param name="prior"></param>
        /// <returns></returns>
        public double[] Update(double[] likelihood, double[] prior)
        {
            for(int i = 0; i < likelihood.Length; i++)
            {
                prior[i] = likelihood[i] * prior[i];
            }

            Normalize(prior);

            return prior;
        }
    }
}
