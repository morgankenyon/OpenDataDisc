namespace OpenDataDisc.Services.Models
{
    public class DiscConfigurationData
    {
        public DiscConfigurationData(
            string deviceId,
            long date,
            double accXOffset,
            double accYOffset,
            double accZOffset)
        {
            DeviceId = deviceId;
            Date = date;
            AccXOffset = accXOffset;
            AccYOffset = accYOffset;
            AccZOffset = accZOffset;
        }

        public string DeviceId { get; }
        public long Date { get; }
        public double AccXOffset { get; }
        public double AccYOffset { get; }
        public double AccZOffset { get; }
    }
}
