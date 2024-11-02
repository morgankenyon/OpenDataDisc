using System;

namespace OpenDataDisc.UI.Filter
{
    public class ImuProcessor
    {
        private ExtendedKalmanFilter _ekf;
        private bool _isInitialized;

        public ImuProcessor()
        {
            _ekf = new ExtendedKalmanFilter();
            _isInitialized = false;

            // Configure noise parameters - adjust these based on your sensor characteristics
            _ekf.SetNoiseParameters(
                processNoisePos: 0.01,     // Position noise (meters)
                processNoiseAngle: 0.01,   // Angle noise (radians)
                processNoiseVel: 0.1,      // Velocity noise (m/s)
                processNoiseOmega: 0.01,   // Angular velocity noise (rad/s)
                measurementNoiseAccel: 0.05, // Accelerometer noise (Gs)
                measurementNoiseGyro: 0.1   // Gyroscope noise (rad/s)
            );
        }

        public ExtendedKalmanState
            ProcessMeasurement(ImuMeasurement measurement)
        {
            if (!_isInitialized)
            {
                InitializeFilter(measurement.AccelX,
                    measurement.AccelY,
                    measurement.AccelZ,
                    measurement.GyroX,
                    measurement.GyroY,
                    measurement.GyroZ);
                return ConvertToState(_ekf.GetState());
            }

            // Create measurement object
            //var measurement = new ImuMeasurement
            //{
            //    // Convert to seconds since epoch for timestamp
            //    Timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerSecond,
            //    AccelX = accelX,
            //    AccelY = accelY,
            //    AccelZ = accelZ,
            //    GyroX = gyroX,
            //    GyroY = gyroY,
            //    GyroZ = gyroZ
            //};

            // Update filter
            _ekf.Update(measurement);

            return ConvertToState(_ekf.GetState());
        }

        private void InitializeFilter(double accelX, double accelY, double accelZ,
                                    double gyroX, double gyroY, double gyroZ)
        {
            // Initialize with rough orientation from accelerometer
            double roll = Math.Atan2(accelY, accelZ);
            double pitch = Math.Atan2(-accelX,
                Math.Sqrt(accelY * accelY + accelZ * accelZ));

            _ekf.SetInitialState(
                x: 0, y: 0, z: 0,           // Start at origin
                roll: roll,                  // Initial roll from accelerometer
                pitch: pitch,                // Initial pitch from accelerometer
                yaw: 0,                      // Can't determine yaw from accelerometer
                vx: 0, vy: 0, vz: 0,        // Assume starting from rest
                omegaX: gyroX,              // Initial angular velocities from gyro
                omegaY: gyroY,
                omegaZ: gyroZ
            );

            _isInitialized = true;
        }

        private ExtendedKalmanState ConvertToState((double x, double y, double z,
                               double roll, double pitch, double yaw,
                               double vx, double vy, double vz,
                               double omegaX, double omegaY, double omegaZ) state)
        {
            return new ExtendedKalmanState
            {
                X = state.x,
                Y = state.y,
                Z = state.z,
                Roll = state.roll,
                Pitch = state.pitch,
                Yaw = state.yaw,
                Vx = state.vx,
                Vy = state.vy,
                Vz = state.vz,
                OmegaX = state.omegaX,
                OmegaY = state.omegaY,
                OmegaZ = state.omegaZ
            };
        }
    }

    public class ExtendedKalmanState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Vz { get; set; }
        public double OmegaX { get; set; }
        public double OmegaY { get; set; }
        public double OmegaZ { get; set; }
    }
}
