using MathNet.Numerics.LinearAlgebra;
using System;

namespace OpenDataDisc.UI.Filter
{
    public class ImuMeasurement
    {
        public uint Timestamp { get; set; }
        public long UptimeMs { get; set; }
        public double AccelX { get; set; }
        public double AccelY { get; set; }
        public double AccelZ { get; set; }
        public double GyroX { get; set; }
        public double GyroY { get; set; }
        public double GyroZ { get; set; }
    }

    public class ExtendedKalmanFilter
    {
        private Matrix<double> _stateEstimate;      // State: [x, y, z, roll, pitch, yaw, vx, vy, vz, ωx, ωy, ωz]
        private Matrix<double> _errorCovariance;    // P
        private Matrix<double> _processNoise;       // Q
        private Matrix<double> _measurementNoise;   // R
        private long _lastTimestamp;
        private readonly int _stateSize = 12;       // [x, y, z, roll, pitch, yaw, vx, vy, vz, ωx, ωy, ωz]
        private readonly int _measurementSize = 6;  // [ax, ay, az, ωx, ωy, ωz]
        private const double GRAVITY = 1;           // In Gs

        public ExtendedKalmanFilter()
        {
            _stateEstimate = Matrix<double>.Build.Dense(_stateSize, 1);
            _errorCovariance = Matrix<double>.Build.DenseIdentity(_stateSize);
            _processNoise = Matrix<double>.Build.DenseIdentity(_stateSize);
            _measurementNoise = Matrix<double>.Build.DenseIdentity(_measurementSize);
            _lastTimestamp = 0;
        }

        public void SetInitialState(
            double x, double y, double z,
            double roll, double pitch, double yaw,
            double vx, double vy, double vz,
            double omegaX, double omegaY, double omegaZ)
        {
            _stateEstimate = Matrix<double>.Build.Dense(_stateSize, 1, new[]
            {
            x, y, z,                     // Position
            roll, pitch, yaw,            // Orientation (Euler angles)
            vx, vy, vz,                  // Linear velocity
            omegaX, omegaY, omegaZ       // Angular velocity
        });
            _errorCovariance = Matrix<double>.Build.DenseIdentity(_stateSize) * 0.1;
        }

        public void SetNoiseParameters(
            double processNoisePos,
            double processNoiseAngle,
            double processNoiseVel,
            double processNoiseOmega,
            double measurementNoiseAccel,
            double measurementNoiseGyro)
        {
            // Process noise
            _processNoise = Matrix<double>.Build.Dense(_stateSize, _stateSize);

            // Position noise
            _processNoise[0, 0] = processNoisePos;
            _processNoise[1, 1] = processNoisePos;
            _processNoise[2, 2] = processNoisePos;

            // Angle noise
            _processNoise[3, 3] = processNoiseAngle;
            _processNoise[4, 4] = processNoiseAngle;
            _processNoise[5, 5] = processNoiseAngle;

            // Velocity noise
            _processNoise[6, 6] = processNoiseVel;
            _processNoise[7, 7] = processNoiseVel;
            _processNoise[8, 8] = processNoiseVel;

            // Angular velocity noise
            _processNoise[9, 9] = processNoiseOmega;
            _processNoise[10, 10] = processNoiseOmega;
            _processNoise[11, 11] = processNoiseOmega;

            // Measurement noise
            _measurementNoise = Matrix<double>.Build.Dense(_measurementSize, _measurementSize);

            // Accelerometer noise
            _measurementNoise[0, 0] = measurementNoiseAccel;
            _measurementNoise[1, 1] = measurementNoiseAccel;
            _measurementNoise[2, 2] = measurementNoiseAccel;

            // Gyroscope noise
            _measurementNoise[3, 3] = measurementNoiseGyro;
            _measurementNoise[4, 4] = measurementNoiseGyro;
            _measurementNoise[5, 5] = measurementNoiseGyro;
        }

        public void Update(ImuMeasurement measurement)
        {
            long dt = _lastTimestamp == 0 ? 1 : measurement.Timestamp - _lastTimestamp;
            _lastTimestamp = measurement.Timestamp;

            PredictState(dt);

            var measurementVector = Matrix<double>.Build.Dense(_measurementSize, 1, new[]
            {
                measurement.AccelX,
                measurement.AccelY,
                measurement.AccelZ,
                measurement.GyroX,
                measurement.GyroY,
                measurement.GyroZ
            });

            UpdateWithMeasurement(measurementVector, dt);
        }

        private void PredictState(double dt)
        {
            var state = _stateEstimate;

            // Current state values
            double roll = state[3, 0];
            double pitch = state[4, 0];
            double yaw = state[5, 0];
            double vx = state[6, 0];
            double vy = state[7, 0];
            double vz = state[8, 0];
            double omegaX = state[9, 0];
            double omegaY = state[10, 0];
            double omegaZ = state[11, 0];

            // Predict next state
            var nextState = Matrix<double>.Build.Dense(_stateSize, 1);

            // Position prediction
            nextState[0, 0] = state[0, 0] + vx * dt;
            nextState[1, 0] = state[1, 0] + vy * dt;
            nextState[2, 0] = state[2, 0] + vz * dt;

            // Euler angle prediction (assuming small angles for simplicity)
            // Note: This is a simplified model. For more accurate results, 
            // you should use the full rotation matrix or quaternions
            nextState[3, 0] = NormalizeAngle(roll + dt * (omegaX +
                             Math.Sin(roll) * Math.Tan(pitch) * omegaY +
                             Math.Cos(roll) * Math.Tan(pitch) * omegaZ));

            nextState[4, 0] = NormalizeAngle(pitch + dt * (Math.Cos(roll) * omegaY -
                             Math.Sin(roll) * omegaZ));

            nextState[5, 0] = NormalizeAngle(yaw + dt * (Math.Sin(roll) / Math.Cos(pitch) * omegaY +
                             Math.Cos(roll) / Math.Cos(pitch) * omegaZ));

            // Velocity prediction (assuming constant velocity model)
            nextState[6, 0] = vx;
            nextState[7, 0] = vy;
            nextState[8, 0] = vz;

            // Angular velocity prediction (assuming constant angular velocity)
            nextState[9, 0] = omegaX;
            nextState[10, 0] = omegaY;
            nextState[11, 0] = omegaZ;

            // Calculate state transition Jacobian
            var F = Matrix<double>.Build.DenseIdentity(_stateSize);

            // Position derivatives
            F[0, 6] = dt;  // dx/dvx
            F[1, 7] = dt;  // dy/dvy
            F[2, 8] = dt;  // dz/dvz

            // Angle derivatives (simplified)
            F[3, 9] = dt;  // droll/domegaX
            F[4, 10] = dt; // dpitch/domegaY
            F[5, 11] = dt; // dyaw/domegaZ

            // Update state and covariance
            _stateEstimate = nextState;
            _errorCovariance = F * _errorCovariance * F.Transpose() + _processNoise * dt;
        }

        private void UpdateWithMeasurement(Matrix<double> measurement, double dt)
        {
            double roll = _stateEstimate[3, 0];
            double pitch = _stateEstimate[4, 0];
            double yaw = _stateEstimate[5, 0];
            double vx = _stateEstimate[6, 0];
            double vy = _stateEstimate[7, 0];
            double vz = _stateEstimate[8, 0];
            double omegaX = _stateEstimate[9, 0];
            double omegaY = _stateEstimate[10, 0];
            double omegaZ = _stateEstimate[11, 0];

            // Calculate expected measurements
            var expectedMeasurement = Matrix<double>.Build.Dense(_measurementSize, 1);

            // Create rotation matrix from Euler angles
            double cr = Math.Cos(roll);
            double sr = Math.Sin(roll);
            double cp = Math.Cos(pitch);
            double sp = Math.Sin(pitch);
            double cy = Math.Cos(yaw);
            double sy = Math.Sin(yaw);

            // Expected accelerations including gravity
            // This is a simplified model - you might want to add centripetal and tangential accelerations
            expectedMeasurement[0, 0] = -GRAVITY * sp;
            expectedMeasurement[1, 0] = GRAVITY * sr * cp;
            expectedMeasurement[2, 0] = GRAVITY * cr * cp;

            // Expected angular velocities
            expectedMeasurement[3, 0] = omegaX;
            expectedMeasurement[4, 0] = omegaY;
            expectedMeasurement[5, 0] = omegaZ;

            // Calculate measurement Jacobian
            var H = Matrix<double>.Build.Dense(_measurementSize, _stateSize);

            // Derivatives for accelerometer measurements with respect to angles
            H[0, 3] = 0;  // dax/droll
            H[0, 4] = -GRAVITY * cp;  // dax/dpitch
            H[0, 5] = 0;  // dax/dyaw

            H[1, 3] = GRAVITY * cr * cp;  // day/droll
            H[1, 4] = -GRAVITY * sr * sp;  // day/dpitch
            H[1, 5] = 0;  // day/dyaw

            H[2, 3] = -GRAVITY * sr * cp;  // daz/droll
            H[2, 4] = -GRAVITY * cr * sp;  // daz/dpitch
            H[2, 5] = 0;  // daz/dyaw

            // Derivatives for gyroscope measurements
            H[3, 9] = 1.0;   // domegaX/domegaX
            H[4, 10] = 1.0;  // domegaY/domegaY
            H[5, 11] = 1.0;  // domegaZ/domegaZ

            // Kalman update
            var S = H * _errorCovariance * H.Transpose() + _measurementNoise;
            var K = _errorCovariance * H.Transpose() * S.Inverse();
            var innovation = measurement - expectedMeasurement;

            // Update state and covariance
            _stateEstimate = _stateEstimate + K * innovation;

            // Normalize angles
            _stateEstimate[3, 0] = NormalizeAngle(_stateEstimate[3, 0]);  // roll
            _stateEstimate[4, 0] = NormalizeAngle(_stateEstimate[4, 0]);  // pitch
            _stateEstimate[5, 0] = NormalizeAngle(_stateEstimate[5, 0]);  // yaw

            var I = Matrix<double>.Build.DenseIdentity(_stateSize);
            _errorCovariance = (I - K * H) * _errorCovariance;
        }

        private double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }

        public (double X, double Y, double Z,             // Position
                double Roll, double Pitch, double Yaw,    // Orientation (radians)
                double Vx, double Vy, double Vz,          // Linear velocity
                double OmegaX, double OmegaY, double OmegaZ) GetState()  // Angular velocity
        {
            return (
                _stateEstimate[0, 0], _stateEstimate[1, 0], _stateEstimate[2, 0],
                _stateEstimate[3, 0], _stateEstimate[4, 0], _stateEstimate[5, 0],
                _stateEstimate[6, 0], _stateEstimate[7, 0], _stateEstimate[8, 0],
                _stateEstimate[9, 0], _stateEstimate[10, 0], _stateEstimate[11, 0]
            );
        }
    }
}
