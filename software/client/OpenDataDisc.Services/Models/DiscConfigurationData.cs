namespace OpenDataDisc.Services.Models
{
    public class DiscConfigurationData
    {
        public DiscConfigurationData(
            string deviceId,
            long date,
            double accXOffset,
            double accYOffset,
            double accZOffset,
            double gyroXOffset,
            double gyroYOffset,
            double gyroZOffset)
        {
            DeviceId = deviceId;
            Date = date;
            AccXOffset = accXOffset;
            AccYOffset = accYOffset;
            AccZOffset = accZOffset;
            GyroXOffset = gyroXOffset;
            GyroYOffset = gyroYOffset;
            GyroZOffset = gyroZOffset;
        }

        public string DeviceId { get; }
        public long Date { get; }
        public double AccXOffset { get; }
        public double AccYOffset { get; }
        public double AccZOffset { get; }
        public double GyroXOffset { get; }
        public double GyroYOffset { get; }
        public double GyroZOffset { get; }
    }
}
