using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDataDisc.Services
{
    public class SensorData
    {
        public SensorData(
            string type,
            int date,
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

        public string Type { get; }
        public int Date { get; }
        public float Val1 { get; }
        public float Val2 { get; }
        public float Val3 { get; }
    }
}
