using InTheHand.Bluetooth;
using System;

namespace OpenDataDisc.UI
{
    public class Constants
    {
        public static readonly BluetoothUuid ServiceUuid = BluetoothUuid.FromGuid(Guid.Parse("900e9509-a0b2-4d89-9bb6-b5e011e758a0"));
        public static readonly BluetoothUuid CharacteristicUuid = BluetoothUuid.FromGuid(Guid.Parse("6ef4cd45-7223-43b2-b5c9-d13410b494a5"));
    }
}
