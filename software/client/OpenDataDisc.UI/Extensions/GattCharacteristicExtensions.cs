using InTheHand.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataDisc.UI.Extensions
{
    public static class GattCharacteristicExtensions
    {
        public async static Task SafeStopNotificationsAsync(this GattCharacteristic? chars)
        {
            if (chars != null)
            {
                try
                {
                    await chars.StopNotificationsAsync();
                }
                catch { }
            }
        }
    }
}
