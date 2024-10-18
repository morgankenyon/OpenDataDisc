using InTheHand.Bluetooth;

namespace OpenDataDisc.UI.Extensions
{
    public static class RemoteGattServerExtensions
    {
        public static void SafeDisconnect(this RemoteGattServer gatt)
        {
            try
            {
                gatt.Disconnect();
            }
            catch { }
        }
    }
}
