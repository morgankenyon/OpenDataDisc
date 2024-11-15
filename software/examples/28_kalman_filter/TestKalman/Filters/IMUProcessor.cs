using System;
using System.Threading;

namespace TestKalman.Filters
{
    public class IMUProcessor
    {
        private readonly CombinedIMUKalmanFilter kalmanFilter;

        public IMUProcessor()
        {
            kalmanFilter = new CombinedIMUKalmanFilter(processNoise: 0.001, measurementNoise: 0.1);
        }

        public void ProcessIMUData(Func<IMUData> getImuData)
        {
            while (true)
            {
                // Get IMU data packet including timestamp
                var imuData = getImuData();

                // Update Kalman filter with IMU data
                var (filteredPitch, filteredRoll) = kalmanFilter.Update(imuData);

                // Output the filtered angles
                Console.WriteLine($"Timestamp: {imuData.Timestamp}ms, " +
                                $"Pitch: {filteredPitch:F2}°, Roll: {filteredRoll:F2}°");

                // Optional: Add a small delay to prevent CPU overload
                Thread.Sleep(1);
            }
        }
    }
}
