namespace TestKalman.Filters
{
    public struct IMUData
    {
        public double Ax { get; set; }
        public double Ay { get; set; }
        public double Az { get; set; }
        public double Gx { get; set; }
        public double Gy { get; set; }
        public double Gz { get; set; }
        public uint Timestamp { get; set; } // Millisecond timestamp from MCU
    }
}
