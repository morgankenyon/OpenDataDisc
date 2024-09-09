using System;

namespace OpenDataDisc.Services.Models
{
    public class SensorData
    {
        public SensorData(
            SensorType type,
            long date,
            float val1,
            float val2,
            float val3)
        {
            Type = type;
            Date = date;
            Val1 = val1;
            Val2 = val2;
            Val3 = val3;
        }

        public SensorData(string sensorString)
        {
            Date = DateTimeOffset.Now.ToUnixTimeSeconds();
            Type = sensorString.StartsWith("A") ? SensorType.Accelerometer : SensorType.Gyroscope;
            string[] measurements = sensorString.Substring(1).Split(',');
            //parse first val
            if (float.TryParse(measurements[0], out float v1))
            {
                Val1 = v1;
            }
            else
            {
                v1 = 0.0f;
            }

            //parse second val
            if (float.TryParse(measurements[1], out float v2))
            {
                Val2 = v2;
            }
            else
            {
                v2 = 0.0f;
            }

            //parse third val
            if (float.TryParse(measurements[2], out float v3))
            {
                Val3 = v3;
            }
            else
            {
                v3 = 0.0f;
            }
        }

        public SensorType Type { get; }
        public long Date { get; }
        public float Val1 { get; }
        public float Val2 { get; }
        public float Val3 { get; }
    }

    public enum SensorType
    {
        Accelerometer = 0,
        Gyroscope = 1
    }
}
