using InTheHand.Bluetooth;
using OpenDataDisc.Services.Models;

namespace OpenDataDisc.UI.Extensions
{
    public static class GattCharacteristicValueChangedEventArgsExtensions
    {
        public static (SensorData? data, string? errorMessage) ExtractSensorData(this GattCharacteristicValueChangedEventArgs e)
        {
            SensorData? sensorData = null;
            string? errorMessage = null;
            if (e.Value != null)
            {
                var strR = System.Text.Encoding.Default.GetString(e.Value);
                var strReceived = strR.Split("\0")[0];

                if (!string.IsNullOrWhiteSpace(strReceived))
                {
                    sensorData = new SensorData(strReceived);
                }
                else
                {
                    errorMessage = $"Received - {e.Value} - {strReceived}";
                }
            }
            else if (e.Error != null)
            {
                errorMessage = $"Received Error - {e.Error.Message}";
            }
            else
            {
                errorMessage = "Received message";
            }

            return (sensorData, errorMessage);
        }
    }
}
