using MathNet.Numerics.LinearAlgebra;
using System;

namespace TestKalman.Filters
{
    public class CombinedIMUKalmanFilter
    {
        private Vector<double> x; // State vector [pitch, pitch_bias, roll, roll_bias]
        private Matrix<double> H; // Measurement matrix
        private Matrix<double> Q; // Process noise covariance
        private Matrix<double> R; // Measurement noise covariance
        private Matrix<double> P; // Error covariance matrix
        private readonly double processNoise;
        private readonly double biasNoise;
        private uint? lastTimestamp;

        public CombinedIMUKalmanFilter(double processNoise = 0.001, double measurementNoise = 0.1, double biasNoise = 0.003)
        {
            this.processNoise = processNoise;
            this.biasNoise = biasNoise;

            // Initialize state vector [pitch, pitch_bias, roll, roll_bias]
            x = Vector<double>.Build.Dense(4);

            // Measurement matrix - we measure pitch and roll directly
            H = Matrix<double>.Build.DenseOfArray(new double[,] {
            { 1, 0, 0, 0 },  // measured pitch
            { 0, 0, 1, 0 }   // measured roll
        });

            // Measurement noise covariance
            R = Matrix<double>.Build.DenseOfArray(new double[,] {
            { measurementNoise, 0 },
            { 0, measurementNoise }
        });

            // Error covariance matrix
            P = Matrix<double>.Build.DenseIdentity(4);
        }

        private Matrix<double> CreateStateTransitionMatrix(double dt)
        {
            return Matrix<double>.Build.DenseOfArray(new double[,] {
            { 1, -dt, 0,   0   },
            { 0,  1,  0,   0   },
            { 0,  0,  1,  -dt  },
            { 0,  0,  0,   1   }
        });
        }

        private Matrix<double> CreateProcessNoiseMatrix(double dt)
        {
            double dtSquared = dt * dt;
            return Matrix<double>.Build.DenseOfArray(new double[,] {
            { processNoise * dtSquared, 0, 0, 0 },
            { 0, biasNoise * dt, 0, 0 },
            { 0, 0, processNoise * dtSquared, 0 },
            { 0, 0, 0, biasNoise * dt }
        });
        }

        private double CalculateDeltaTime(uint currentTimestamp)
        {
            if (!lastTimestamp.HasValue)
            {
                lastTimestamp = currentTimestamp;
                return 0.001; // Return 1ms for first reading
            }

            // Handle timer wraparound (assuming 32-bit millisecond timer)
            uint deltaTime;
            if (currentTimestamp < lastTimestamp.Value)
            {
                deltaTime = (uint.MaxValue - lastTimestamp.Value) + currentTimestamp;
            }
            else
            {
                deltaTime = currentTimestamp - lastTimestamp.Value;
            }

            lastTimestamp = currentTimestamp;

            // Convert to seconds and ensure we have a reasonable value
            double dt = deltaTime / 1000.0;

            // Sanity check on dt
            if (dt > 1.0) // More than 1 second between samples
            {
                Console.WriteLine($"Warning: Large time step detected: {dt:F3}s");
                dt = 0.1; // Use a reasonable default
            }
            else if (dt <= 0)
            {
                dt = 0.001; // Minimum 1ms
            }

            return dt;
        }

        public (double Pitch, double Roll) Update(IMUData imuData)
        {
            double dt = CalculateDeltaTime(imuData.Timestamp);

            // Calculate angles from accelerometer
            double measuredPitch = Math.Atan2(-imuData.Ax,
                Math.Sqrt(imuData.Ay * imuData.Ay + imuData.Az * imuData.Az)) * 180.0 / Math.PI;
            double measuredRoll = Math.Atan2(imuData.Ay, imuData.Az) * 180.0 / Math.PI;

            // Create matrices that depend on dt
            var A = CreateStateTransitionMatrix(dt);
            var Q = CreateProcessNoiseMatrix(dt);

            // Clip measurements to valid range
            measuredPitch = Math.Clamp(measuredPitch, -60, 60);
            measuredRoll = Math.Clamp(measuredRoll, -60, 60);

            // Predict
            x = A.Multiply(x);
            x[0] += imuData.Gy * dt;  // Integrate pitch rate
            x[2] += imuData.Gx * dt;  // Integrate roll rate
            P = A.Multiply(P).Multiply(A.Transpose()).Add(Q);

            // Create measurement vector
            var z = Vector<double>.Build.Dense(new[] { measuredPitch, measuredRoll });

            // Update
            var y = z.Subtract(H.Multiply(x));
            var S = H.Multiply(P).Multiply(H.Transpose()).Add(R);
            var K = P.Multiply(H.Transpose()).Multiply(S.Inverse());

            x = x.Add(K.Multiply(y));
            P = P.Subtract(K.Multiply(H).Multiply(P));

            return (x[0], x[2]); // Return pitch and roll
        }
    }
}
