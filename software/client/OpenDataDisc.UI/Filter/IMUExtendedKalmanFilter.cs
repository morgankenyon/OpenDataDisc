using MathNet.Numerics.LinearAlgebra;
using System;

namespace OpenDataDisc.UI.Filter
{
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics.LinearAlgebra;

    public class IMUExtendedKalmanFilter
    {
        private Matrix<double> state;              // State vector [roll, pitch, rollRate, pitchRate] (all in radians)
        private Matrix<double> errorCovariance;    // Error covariance
        private readonly double processNoiseScale; // Base process noise scaling factor
        private readonly double measurementNoise;  // Measurement noise
        private uint lastUpdateTime;           // Time of last update
        private bool isInitialized;               // Flag to handle first measurement
        private const double DEG_TO_RAD = Math.PI / 180.0;
        private const double RAD_TO_DEG = 180.0 / Math.PI;
        private readonly bool gyroInDegrees;      // Flag indicating if gyro measurements are in degrees/sec

        // Shorter moving average for better responsiveness
        private const int MOVING_AVERAGE_LENGTH = 1;
        private Queue<(double X, double Y, double Z)> accHistory;

        public IMUExtendedKalmanFilter(bool gyroInDegrees = true,
            double processNoiseScale = 0.1,
            double measurementNoise = 0.1)
        {
            // Initialize state vector [roll, pitch, rollRate, pitchRate] (all in radians)
            state = Matrix<double>.Build.Dense(4, 1);

            // Lower initial uncertainty to trust measurements more
            errorCovariance = Matrix<double>.Build.DenseIdentity(4) * 1.0;

            this.processNoiseScale = processNoiseScale;
            this.measurementNoise = measurementNoise;
            this.isInitialized = false;
            this.gyroInDegrees = gyroInDegrees;

            accHistory = new Queue<(double X, double Y, double Z)>();
        }

        public (double RollDegrees, double PitchDegrees, uint timestamp) Update(
            uint timestamp,
            double accXg, double accYg, double accZg,  // Accelerometer readings in Gs
            double gyroX, double gyroY)                // Gyro readings in deg/s or rad/s based on constructor flag
        {
            // Basic input validation
            if (double.IsNaN(accXg) || double.IsNaN(accYg) || double.IsNaN(accZg) ||
                double.IsInfinity(accXg) || double.IsInfinity(accYg) || double.IsInfinity(accZg))
            {
                lastUpdateTime = timestamp;
                return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG, lastUpdateTime);
            }

            // Calculate magnitude but don't normalize unless really necessary
            //double magnitude = Math.Sqrt(accXg * accXg + accYg * accYg + accZg * accZg);
            //if (magnitude < 0.5 || magnitude > 1.5)
            //{
            //    // Only normalize if significantly off from 1G
            //    accXg /= magnitude;
            //    accYg /= magnitude;
            //    accZg /= magnitude;
            //}

            // Update moving average
            accHistory.Enqueue((accXg, accYg, accZg));
            if (accHistory.Count > MOVING_AVERAGE_LENGTH)
            {
                accHistory.Dequeue();
            }

            // Calculate smoothed accelerometer readings
            var avgAcc = CalculateAverageAccelerometer();

            // Convert gyro measurements to rad/s if needed
            double gyroXrad = gyroInDegrees ? gyroX * DEG_TO_RAD : gyroX;
            double gyroYrad = gyroInDegrees ? gyroY * DEG_TO_RAD : gyroY;

            // Handle first measurement
            if (!isInitialized)
            {
                lastUpdateTime = timestamp;
                var (accRoll, accPitch) = CalculateAccelerometerAngles(avgAcc.X, avgAcc.Y, avgAcc.Z);

                state[0, 0] = accRoll;
                state[1, 0] = accPitch;
                state[2, 0] = gyroXrad;
                state[3, 0] = gyroYrad;

                isInitialized = true;
                return (accRoll * RAD_TO_DEG, accPitch * RAD_TO_DEG, lastUpdateTime);
            }

            uint dt = timestamp - lastUpdateTime;
            if (dt <= 0)
            {
                return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG, lastUpdateTime);
            }

            // Predict step
            var predictedState = PredictState(dt);
            var F = CalculateStateTransitionJacobian(dt);
            var Q = CalculateProcessNoise(dt);
            errorCovariance = F * errorCovariance * F.Transpose() + Q;

            // Create measurement vector using smoothed accelerometer data
            var measurement = Matrix<double>.Build.Dense(6, 1);
            measurement[0, 0] = avgAcc.X;
            measurement[1, 0] = avgAcc.Y;
            measurement[2, 0] = avgAcc.Z;
            measurement[3, 0] = gyroXrad;
            measurement[4, 0] = gyroYrad;

            // Update step
            var H = CalculateMeasurementJacobian(predictedState);
            var predictedMeasurement = PredictMeasurement(predictedState);
            var R = CalculateMeasurementNoise(dt);

            var innovation = measurement - predictedMeasurement;
            var S = H * errorCovariance * H.Transpose() + R;
            var K = errorCovariance * H.Transpose() * S.Inverse();

            // Update state
            state = predictedState + K * innovation;

            // Update error covariance
            var I = Matrix<double>.Build.DenseIdentity(4);
            errorCovariance = (I - K * H) * errorCovariance;

            lastUpdateTime = timestamp;
            return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG, lastUpdateTime);
        }

        private Matrix<double> PredictState(double dt)
        {
            var predictedState = Matrix<double>.Build.Dense(4, 1);

            predictedState[0, 0] = state[0, 0] + state[2, 0] * dt;
            predictedState[1, 0] = state[1, 0] + state[3, 0] * dt;
            predictedState[2, 0] = state[2, 0];
            predictedState[3, 0] = state[3, 0];

            return predictedState;
        }

        private Matrix<double> PredictMeasurement(Matrix<double> state)
        {
            var predicted = Matrix<double>.Build.Dense(6, 1);

            double roll = state[0, 0];
            double pitch = state[1, 0];

            predicted[0, 0] = Math.Sin(pitch);
            predicted[1, 0] = -Math.Cos(pitch) * Math.Sin(roll);
            predicted[2, 0] = Math.Cos(pitch) * Math.Cos(roll);
            predicted[3, 0] = state[2, 0];
            predicted[4, 0] = state[3, 0];

            return predicted;
        }

        private Matrix<double> CalculateMeasurementJacobian(Matrix<double> state)
        {
            var H = Matrix<double>.Build.Dense(6, 4);

            double roll = state[0, 0];
            double pitch = state[1, 0];

            H[0, 0] = 0;
            H[0, 1] = Math.Cos(pitch);

            H[1, 0] = -Math.Cos(pitch) * Math.Cos(roll);
            H[1, 1] = Math.Sin(pitch) * Math.Sin(roll);

            H[2, 0] = -Math.Cos(pitch) * Math.Sin(roll);
            H[2, 1] = -Math.Sin(pitch) * Math.Cos(roll);

            H[3, 2] = 1;
            H[4, 3] = 1;

            return H;
        }

        private Matrix<double> CalculateStateTransitionJacobian(double dt)
        {
            var F = Matrix<double>.Build.DenseIdentity(4);
            F[0, 2] = dt;
            F[1, 3] = dt;
            return F;
        }

        private Matrix<double> CalculateProcessNoise(double dt)
        {
            var Q = Matrix<double>.Build.Dense(4, 4);

            // Simplified process noise model
            Q[0, 0] = dt * dt;  // roll
            Q[1, 1] = dt * dt;  // pitch
            Q[2, 2] = dt;       // roll rate
            Q[3, 3] = dt;       // pitch rate

            return Q * processNoiseScale;
        }

        private Matrix<double> CalculateMeasurementNoise(double dt)
        {
            // Simplified measurement noise
            return Matrix<double>.Build.DenseIdentity(6) * measurementNoise;
        }

        private (double X, double Y, double Z) CalculateAverageAccelerometer()
        {
            if (accHistory.Count == 0)
                return (0, 0, 1);

            double sumX = 0, sumY = 0, sumZ = 0;
            foreach (var (x, y, z) in accHistory)
            {
                sumX += x;
                sumY += y;
                sumZ += z;
            }

            return (
                sumX / accHistory.Count,
                sumY / accHistory.Count,
                sumZ / accHistory.Count
            );
        }

        private (double Roll, double Pitch) CalculateAccelerometerAngles(
            double accX, double accY, double accZ)
        {
            double roll = Math.Atan2(accY, accZ);
            double pitch = Math.Atan2(-accX, Math.Sqrt(accY * accY + accZ * accZ));
            return (roll, pitch);
        }

        public (double RollDegrees, double PitchDegrees, double RollRateDegPerSec, double PitchRateDegPerSec) GetStateInDegrees()
        {
            return (
                state[0, 0] * RAD_TO_DEG,
                state[1, 0] * RAD_TO_DEG,
                state[2, 0] * RAD_TO_DEG,
                state[3, 0] * RAD_TO_DEG
            );
        }
    }
}
