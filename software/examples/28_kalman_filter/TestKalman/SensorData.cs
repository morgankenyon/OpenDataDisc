using System.Collections.Generic;

namespace TestKalman
{
    public class SensorData
    {
        public string stmt { get; set; }
        public List<string> header { get; set; }
        public List<List<string>> rows { get; set; }
    }
}
