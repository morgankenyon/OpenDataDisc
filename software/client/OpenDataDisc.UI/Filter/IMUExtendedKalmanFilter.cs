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
        private long lastUpdateTime;           // Time of last update
        private bool isInitialized;               // Flag to handle first measurement
        private const double DEG_TO_RAD = Math.PI / 180.0;
        private const double RAD_TO_DEG = 180.0 / Math.PI;
        private readonly bool gyroInDegrees;      // Flag indicating if gyro measurements are in degrees/sec

        // Shorter moving average for better responsiveness
        private const int MOVING_AVERAGE_LENGTH = 3;
        private Queue<(double X, double Y, double Z)> accHistory;

        public IMUExtendedKalmanFilter(bool gyroInDegrees = true, double processNoiseScale = 0.1, double measurementNoise = 0.1)
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

        public (double RollDegrees, double PitchDegrees) Update(
            long timestamp,
            double accXg, double accYg, double accZg,  // Accelerometer readings in Gs
            double gyroX, double gyroY)                // Gyro readings in deg/s or rad/s based on constructor flag
        {
            // Basic input validation
            if (double.IsNaN(accXg) || double.IsNaN(accYg) || double.IsNaN(accZg) ||
                double.IsInfinity(accXg) || double.IsInfinity(accYg) || double.IsInfinity(accZg))
            {
                return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG);
            }

            // Calculate magnitude but don't normalize unless really necessary
            double magnitude = Math.Sqrt(accXg * accXg + accYg * accYg + accZg * accZg);
            if (magnitude < 0.5 || magnitude > 1.5)
            {
                // Only normalize if significantly off from 1G
                accXg /= magnitude;
                accYg /= magnitude;
                accZg /= magnitude;
            }

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
                return (accRoll * RAD_TO_DEG, accPitch * RAD_TO_DEG);
            }

            double dt = timestamp - lastUpdateTime;
            if (dt <= 0) return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG);

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
            return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG);
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

        //private bool ValidateAccelerometerReadings(double accX, double accY, double accZ,
        //    out double cleanAccX, out double cleanAccY, out double cleanAccZ)
        //{
        //    cleanAccX = accX;
        //    cleanAccY = accY;
        //    cleanAccZ = accZ;

        //    // Check for NaN or Infinity
        //    if (double.IsNaN(accX) || double.IsNaN(accY) || double.IsNaN(accZ) ||
        //        double.IsInfinity(accX) || double.IsInfinity(accY) || double.IsInfinity(accZ))
        //    {
        //        return false;
        //    }

        //    // Calculate total acceleration magnitude
        //    double magnitude = Math.Sqrt(accX * accX + accY * accY + accZ * accZ);

        //    // Check if magnitude is reasonable (should be close to 1G when stationary)
        //    if (magnitude < 0.5 || magnitude > 1.5)
        //    {
        //        return false;
        //    }

        //    // Normalize readings if they're valid but not exactly 1G
        //    cleanAccX = accX / magnitude;
        //    cleanAccY = accY / magnitude;
        //    cleanAccZ = accZ / magnitude;

        //    return true;
        //}

        //private double CleanGyroReading(double gyroReading, bool inDegrees)
        //{
        //    // Convert to rad/s if needed
        //    double gyroRad = inDegrees ? gyroReading * DEG_TO_RAD : gyroReading;

        //    // Limit maximum gyro rate (e.g., ±20 rad/s or about ±1145 deg/s)
        //    double maxRate = 20.0;
        //    return Math.Clamp(gyroRad, -maxRate, maxRate);
        //}

        //private (double X, double Y, double Z) CalculateAverageAccelerometer()
        //{
        //    if (accHistory.Count == 0)
        //        return (0, 0, 1);  // Default to gravity along Z-axis

        //    double sumX = 0, sumY = 0, sumZ = 0;
        //    foreach (var (x, y, z) in accHistory)
        //    {
        //        sumX += x;
        //        sumY += y;
        //        sumZ += z;
        //    }

        //    return (
        //        sumX / accHistory.Count,
        //        sumY / accHistory.Count,
        //        sumZ / accHistory.Count
        //    );
        //}

        //private bool IsValidStateUpdate(Matrix<double> update)
        //{
        //    // Check if any state updates are too large
        //    for (int i = 0; i < update.RowCount; i++)
        //    {
        //        if (Math.Abs(update[i, 0]) > Math.PI)  // More than 180 degrees change
        //            return false;
        //    }
        //    return true;
        //}

        //private double ClampAngle(double angle)
        //{
        //    // Clamp angle to valid range (-MAX_VALID_ANGLE to +MAX_VALID_ANGLE)
        //    return Math.Clamp(angle, -MAX_VALID_ANGLE, MAX_VALID_ANGLE);
        //}




        

        //public (double RollDegrees, double PitchDegrees, double RollRateDegPerSec, double PitchRateDegPerSec) GetStateInDegrees()
        //{
        //    return (
        //        state[0, 0] * RAD_TO_DEG,    // Roll in degrees
        //        state[1, 0] * RAD_TO_DEG,    // Pitch in degrees
        //        state[2, 0] * RAD_TO_DEG,    // Roll rate in deg/s
        //        state[3, 0] * RAD_TO_DEG     // Pitch rate in deg/s
        //    );
        //}

        //public (double RollRad, double PitchRad, double RollRateRadPerSec, double PitchRateRadPerSec) GetStateInRadians()
        //{
        //    return (
        //        state[0, 0],    // Roll in radians
        //        state[1, 0],    // Pitch in radians
        //        state[2, 0],    // Roll rate in rad/s
        //        state[3, 0]     // Pitch rate in rad/s
        //    );
        //}

        //private Matrix<double> PredictState(double dt)
        //{
        //    var predictedState = Matrix<double>.Build.Dense(4, 1);

        //    predictedState[0, 0] = state[0, 0] + state[2, 0] * dt;  // roll + rollRate * dt
        //    predictedState[1, 0] = state[1, 0] + state[3, 0] * dt;  // pitch + pitchRate * dt
        //    predictedState[2, 0] = state[2, 0];  // roll rate (assumed constant over dt)
        //    predictedState[3, 0] = state[3, 0];  // pitch rate (assumed constant over dt)

        //    return predictedState;
        //}

        //private Matrix<double> PredictMeasurement(Matrix<double> state)
        //{
        //    var predicted = Matrix<double>.Build.Dense(6, 1);

        //    // Convert roll and pitch to expected accelerometer readings in Gs
        //    double roll = state[0, 0];
        //    double pitch = state[1, 0];

        //    // Expected accelerometer readings in Gs (1G = 9.81 m/s²)
        //    predicted[0, 0] = Math.Sin(pitch);                        // accX in Gs
        //    predicted[1, 0] = -Math.Cos(pitch) * Math.Sin(roll);     // accY in Gs
        //    predicted[2, 0] = Math.Cos(pitch) * Math.Cos(roll);      // accZ in Gs

        //    // Gyroscope readings (already in rad/s)
        //    predicted[3, 0] = state[2, 0];  // gyroX
        //    predicted[4, 0] = state[3, 0];  // gyroY

        //    return predicted;
        //}

        //private Matrix<double> CalculateMeasurementJacobian(Matrix<double> state)
        //{
        //    var H = Matrix<double>.Build.Dense(6, 4);

        //    double roll = state[0, 0];
        //    double pitch = state[1, 0];

        //    // Derivatives for accelerometer measurements (in Gs)
        //    // ∂(accX)/∂roll = 0
        //    H[0, 0] = 0;
        //    // ∂(accX)/∂pitch = cos(pitch)
        //    H[0, 1] = Math.Cos(pitch);

        //    // ∂(accY)/∂roll = -cos(pitch) * cos(roll)
        //    H[1, 0] = -Math.Cos(pitch) * Math.Cos(roll);
        //    // ∂(accY)/∂pitch = sin(pitch) * sin(roll)
        //    H[1, 1] = Math.Sin(pitch) * Math.Sin(roll);

        //    // ∂(accZ)/∂roll = -cos(pitch) * sin(roll)
        //    H[2, 0] = -Math.Cos(pitch) * Math.Sin(roll);
        //    // ∂(accZ)/∂pitch = -sin(pitch) * cos(roll)
        //    H[2, 1] = -Math.Sin(pitch) * Math.Cos(roll);

        //    // Gyroscope measurements (rad/s)
        //    H[3, 2] = 1;  // ∂(gyroX)/∂rollRate = 1
        //    H[4, 3] = 1;  // ∂(gyroY)/∂pitchRate = 1

        //    return H;
        //}

        //private Matrix<double> CalculateStateTransitionJacobian(double dt)
        //{
        //    var F = Matrix<double>.Build.DenseIdentity(4);
        //    F[0, 2] = dt;  // ∂(roll)/∂(rollRate)
        //    F[1, 3] = dt;  // ∂(pitch)/∂(pitchRate)
        //    return F;
        //}

        //private Matrix<double> CalculateProcessNoise(double dt)
        //{
        //    var Q = Matrix<double>.Build.Dense(4, 4);

        //    // Position elements (roll and pitch)
        //    Q[0, 0] = Math.Pow(dt, 3) / 3;  // roll
        //    Q[1, 1] = Math.Pow(dt, 3) / 3;  // pitch

        //    // Velocity elements (roll rate and pitch rate)
        //    Q[2, 2] = dt;  // roll rate
        //    Q[3, 3] = dt;  // pitch rate

        //    // Cross terms
        //    Q[0, 2] = Q[2, 0] = Math.Pow(dt, 2) / 2;  // roll and roll rate
        //    Q[1, 3] = Q[3, 1] = Math.Pow(dt, 2) / 2;  // pitch and pitch rate

        //    return Q * processNoiseScale;
        //}

        //private Matrix<double> CalculateMeasurementNoise(double dt)
        //{
        //    var R = Matrix<double>.Build.DenseIdentity(6) * measurementNoise;

        //    // Increase accelerometer noise with dt
        //    for (int i = 0; i < 3; i++)
        //    {
        //        R[i, i] *= (1 + dt);  // Accelerometer measurements become less reliable over time
        //    }

        //    return R;
        //}

        //private (double Roll, double Pitch) CalculateAccelerometerAngles(
        //    double accXg, double accYg, double accZg)  // Inputs in Gs
        //{
        //    // Returns angles in radians since we work in radians internally
        //    double roll = Math.Atan2(accYg, accZg);
        //    double pitch = Math.Atan2(-accXg, Math.Sqrt(accYg * accYg + accZg * accZg));
        //    return (roll, pitch);
        //}
    }
}
