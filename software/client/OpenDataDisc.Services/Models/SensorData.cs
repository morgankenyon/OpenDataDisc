using System;

namespace OpenDataDisc.Services.Models
{
    public class SensorData
    {
        public SensorData(
            long date,
            float accX,
            float accY,
            float accZ,
            float gyroX,
            float gyroY,
            float gyroZ)
        {
            Date = date;
            AccX = accX;
            AccY = accY;
            AccZ = accZ;
            GyroX = gyroX;
            GyroY = gyroY;
            GyroZ = gyroZ;
        }

        private float parseValue(string value)
        {
            float val = 0.0f;
            if (float.TryParse(value, out float parsedValue))
            {
                val = parsedValue;
            }
            return val;
        }

        public SensorData(string sensorString)
        {
            sensorString = sensorString.Trim();
            Date = DateTimeOffset.Now.ToUnixTimeSeconds();
            string[] measurements = sensorString.Split(';');
            string[] accelerometerValues = measurements[0].Split(',');
            string[] gyroValues = measurements[1].Split(',');

            //parse accelerometers
            AccX = parseValue(accelerometerValues[0]);
            AccY = parseValue(accelerometerValues[1]);
            AccZ = parseValue(accelerometerValues[2]);

            //parse accelerometers
            GyroX = parseValue(gyroValues[0]);
            GyroY = parseValue(gyroValues[1]);
            GyroZ = parseValue(gyroValues[2]);
        }

        public long Date { get; }
        public float AccX { get; }
        public float AccY { get; }
        public float AccZ { get; }
        public float GyroX { get; }
        public float GyroY { get; }
        public float GyroZ { get; }
    }
}
