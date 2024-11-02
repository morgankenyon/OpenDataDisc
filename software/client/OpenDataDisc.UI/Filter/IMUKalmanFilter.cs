using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataDisc.UI.Filter
{
    public class IMUKalmanFilter
    {
        private Matrix<double> X; // State: [roll, pitch, roll_rate, pitch_rate]
        private Matrix<double> P; // Estimate covariance
        private Matrix<double> Q; // Process noise covariance
        private Matrix<double> R; // Measurement noise covariance
        private double dt; // Time step

        public IMUKalmanFilter(double timeStep)
        {
            dt = timeStep;
            X = Matrix<double>.Build.Dense(4, 1);
            P = Matrix<double>.Build.DenseIdentity(4);
            Q = Matrix<double>.Build.DenseIdentity(4) * 0.001; // Adjust as needed
            R = Matrix<double>.Build.DenseIdentity(2) * 0.1;   // Adjust as needed
        }

        private Matrix<double> f(Matrix<double> x, Matrix<double> gyro)
        {
            var roll = x[0, 0];
            var pitch = x[1, 0];
            var rollRate = gyro[0, 0];
            var pitchRate = gyro[1, 0];

            var newState = Matrix<double>.Build.Dense(4, 1);
            newState[0, 0] = roll + dt * rollRate;
            newState[1, 0] = pitch + dt * pitchRate;
            newState[2, 0] = rollRate;
            newState[3, 0] = pitchRate;

            return newState;
        }

        private Matrix<double> h(Matrix<double> x)
        {
            var roll = x[0, 0];
            var pitch = x[1, 0];

            var measurement = Matrix<double>.Build.Dense(2, 1);
            measurement[0, 0] = roll;
            measurement[1, 0] = pitch;

            return measurement;
        }

        private Matrix<double> calculateJacobianF(Matrix<double> x, Matrix<double> gyro)
        {
            var F = Matrix<double>.Build.DenseIdentity(4);
            F[0, 2] = dt;
            F[1, 3] = dt;
            return F;
        }

        private Matrix<double> calculateJacobianH()
        {
            var H = Matrix<double>.Build.Dense(2, 4);
            H[0, 0] = 1;
            H[1, 1] = 1;
            return H;
        }

        public void Predict(Matrix<double> gyro)
        {
            // Predict state
            X = f(X, gyro);

            // Calculate Jacobian of f
            var F = calculateJacobianF(X, gyro);

            // Predict covariance
            P = F * P * F.Transpose() + Q;
        }

        public void Update(Matrix<double> accel)
        {
            // Calculate roll and pitch from accelerometer data
            var roll = Math.Atan2(accel[1, 0], accel[2, 0]);
            var pitch = Math.Atan2(-accel[0, 0], Math.Sqrt(accel[1, 0] * accel[1, 0] + accel[2, 0] * accel[2, 0]));

            var measurement = Matrix<double>.Build.Dense(2, 1);
            measurement[0, 0] = roll;
            measurement[1, 0] = pitch;

            // Calculate Jacobian of h
            var H = calculateJacobianH();

            // Calculate Kalman gain
            var S = H * P * H.Transpose() + R;
            var K = P * H.Transpose() * S.Inverse();

            // Update state
            X = X + K * (measurement - h(X));

            // Update covariance
            var I = Matrix<double>.Build.DenseIdentity(4);
            P = (I - K * H) * P;
        }

        public (double Roll, double Pitch) GetAngles()
        {
            return (X[0, 0], X[1, 0]);
        }
    }
}
