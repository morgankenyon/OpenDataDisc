using MathNet.Numerics.LinearAlgebra;
using System;

namespace OpenDataDisc.UI.Filter
{
    using System;
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

        public IMUExtendedKalmanFilter(bool gyroInDegrees = true, double processNoiseScale = 0.001, double measurementNoise = 0.1)
        {
            // Initialize state vector [roll, pitch, rollRate, pitchRate] (all in radians)
            state = Matrix<double>.Build.Dense(4, 1);

            // Initialize error covariance matrix with high initial uncertainty
            errorCovariance = Matrix<double>.Build.DenseIdentity(4) * 1.0;

            this.processNoiseScale = processNoiseScale;
            this.measurementNoise = measurementNoise;
            this.isInitialized = false;
            this.gyroInDegrees = gyroInDegrees;
        }

        public (double RollDegrees, double PitchDegrees) Update(
            long timestamp,
            double accXg, double accYg, double accZg,  // Accelerometer readings in Gs
            double gyroX, double gyroY)                // Gyro readings in deg/s or rad/s based on constructor flag
        {
            // Convert gyro measurements to rad/s if they're in deg/s
            double gyroXrad = gyroInDegrees ? gyroX * DEG_TO_RAD : gyroX;
            double gyroYrad = gyroInDegrees ? gyroY * DEG_TO_RAD : gyroY;

            // Handle first measurement
            if (!isInitialized)
            {
                lastUpdateTime = timestamp;
                var (accRoll, accPitch) = CalculateAccelerometerAngles(accXg, accYg, accZg);

                state[0, 0] = accRoll;           // roll in radians
                state[1, 0] = accPitch;          // pitch in radians
                state[2, 0] = gyroXrad;          // roll rate in rad/s
                state[3, 0] = gyroYrad;          // pitch rate in rad/s

                isInitialized = true;
                return (accRoll * RAD_TO_DEG, accPitch * RAD_TO_DEG);
            }

            double dt = timestamp - lastUpdateTime;
            if (dt <= 0) return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG);

            // EKF Prediction Step
            var predictedState = PredictState(dt);
            var F = CalculateStateTransitionJacobian(dt);
            var Q = CalculateProcessNoise(dt);
            errorCovariance = F * errorCovariance * F.Transpose() + Q;

            // Create measurement vector
            var measurement = Matrix<double>.Build.Dense(6, 1);
            measurement[0, 0] = accXg;      // X acceleration in Gs
            measurement[1, 0] = accYg;      // Y acceleration in Gs
            measurement[2, 0] = accZg;      // Z acceleration in Gs
            measurement[3, 0] = gyroXrad;   // X angular rate in rad/s
            measurement[4, 0] = gyroYrad;   // Y angular rate in rad/s

            // EKF Update Step
            var H = CalculateMeasurementJacobian(predictedState);
            var predictedMeasurement = PredictMeasurement(predictedState);
            var R = CalculateMeasurementNoise(dt);

            var innovation = measurement - predictedMeasurement;
            var S = H * errorCovariance * H.Transpose() + R;
            var K = errorCovariance * H.Transpose() * S.Inverse();

            state = predictedState + K * innovation;
            var I = Matrix<double>.Build.DenseIdentity(4);
            errorCovariance = (I - K * H) * errorCovariance;

            lastUpdateTime = timestamp;

            // Return angles in degrees
            return (state[0, 0] * RAD_TO_DEG, state[1, 0] * RAD_TO_DEG);
        }

        public (double RollDegrees, double PitchDegrees, double RollRateDegPerSec, double PitchRateDegPerSec) GetStateInDegrees()
        {
            return (
                state[0, 0] * RAD_TO_DEG,    // Roll in degrees
                state[1, 0] * RAD_TO_DEG,    // Pitch in degrees
                state[2, 0] * RAD_TO_DEG,    // Roll rate in deg/s
                state[3, 0] * RAD_TO_DEG     // Pitch rate in deg/s
            );
        }

        public (double RollRad, double PitchRad, double RollRateRadPerSec, double PitchRateRadPerSec) GetStateInRadians()
        {
            return (
                state[0, 0],    // Roll in radians
                state[1, 0],    // Pitch in radians
                state[2, 0],    // Roll rate in rad/s
                state[3, 0]     // Pitch rate in rad/s
            );
        }

        private Matrix<double> PredictState(double dt)
        {
            var predictedState = Matrix<double>.Build.Dense(4, 1);

            predictedState[0, 0] = state[0, 0] + state[2, 0] * dt;  // roll + rollRate * dt
            predictedState[1, 0] = state[1, 0] + state[3, 0] * dt;  // pitch + pitchRate * dt
            predictedState[2, 0] = state[2, 0];  // roll rate (assumed constant over dt)
            predictedState[3, 0] = state[3, 0];  // pitch rate (assumed constant over dt)

            return predictedState;
        }

        private Matrix<double> PredictMeasurement(Matrix<double> state)
        {
            var predicted = Matrix<double>.Build.Dense(6, 1);

            // Convert roll and pitch to expected accelerometer readings in Gs
            double roll = state[0, 0];
            double pitch = state[1, 0];

            // Expected accelerometer readings in Gs (1G = 9.81 m/s²)
            predicted[0, 0] = Math.Sin(pitch);                        // accX in Gs
            predicted[1, 0] = -Math.Cos(pitch) * Math.Sin(roll);     // accY in Gs
            predicted[2, 0] = Math.Cos(pitch) * Math.Cos(roll);      // accZ in Gs

            // Gyroscope readings (already in rad/s)
            predicted[3, 0] = state[2, 0];  // gyroX
            predicted[4, 0] = state[3, 0];  // gyroY

            return predicted;
        }

        private Matrix<double> CalculateMeasurementJacobian(Matrix<double> state)
        {
            var H = Matrix<double>.Build.Dense(6, 4);

            double roll = state[0, 0];
            double pitch = state[1, 0];

            // Derivatives for accelerometer measurements (in Gs)
            // ∂(accX)/∂roll = 0
            H[0, 0] = 0;
            // ∂(accX)/∂pitch = cos(pitch)
            H[0, 1] = Math.Cos(pitch);

            // ∂(accY)/∂roll = -cos(pitch) * cos(roll)
            H[1, 0] = -Math.Cos(pitch) * Math.Cos(roll);
            // ∂(accY)/∂pitch = sin(pitch) * sin(roll)
            H[1, 1] = Math.Sin(pitch) * Math.Sin(roll);

            // ∂(accZ)/∂roll = -cos(pitch) * sin(roll)
            H[2, 0] = -Math.Cos(pitch) * Math.Sin(roll);
            // ∂(accZ)/∂pitch = -sin(pitch) * cos(roll)
            H[2, 1] = -Math.Sin(pitch) * Math.Cos(roll);

            // Gyroscope measurements (rad/s)
            H[3, 2] = 1;  // ∂(gyroX)/∂rollRate = 1
            H[4, 3] = 1;  // ∂(gyroY)/∂pitchRate = 1

            return H;
        }

        private Matrix<double> CalculateStateTransitionJacobian(double dt)
        {
            var F = Matrix<double>.Build.DenseIdentity(4);
            F[0, 2] = dt;  // ∂(roll)/∂(rollRate)
            F[1, 3] = dt;  // ∂(pitch)/∂(pitchRate)
            return F;
        }

        private Matrix<double> CalculateProcessNoise(double dt)
        {
            var Q = Matrix<double>.Build.Dense(4, 4);

            // Position elements (roll and pitch)
            Q[0, 0] = Math.Pow(dt, 3) / 3;  // roll
            Q[1, 1] = Math.Pow(dt, 3) / 3;  // pitch

            // Velocity elements (roll rate and pitch rate)
            Q[2, 2] = dt;  // roll rate
            Q[3, 3] = dt;  // pitch rate

            // Cross terms
            Q[0, 2] = Q[2, 0] = Math.Pow(dt, 2) / 2;  // roll and roll rate
            Q[1, 3] = Q[3, 1] = Math.Pow(dt, 2) / 2;  // pitch and pitch rate

            return Q * processNoiseScale;
        }

        private Matrix<double> CalculateMeasurementNoise(double dt)
        {
            var R = Matrix<double>.Build.DenseIdentity(6) * measurementNoise;

            // Increase accelerometer noise with dt
            for (int i = 0; i < 3; i++)
            {
                R[i, i] *= (1 + dt);  // Accelerometer measurements become less reliable over time
            }

            return R;
        }

        private (double Roll, double Pitch) CalculateAccelerometerAngles(
            double accXg, double accYg, double accZg)  // Inputs in Gs
        {
            // Returns angles in radians since we work in radians internally
            double roll = Math.Atan2(accYg, accZg);
            double pitch = Math.Atan2(-accXg, Math.Sqrt(accYg * accYg + accZg * accZg));
            return (roll, pitch);
        }
    }
}
