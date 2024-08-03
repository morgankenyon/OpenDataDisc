using InTheHand.Bluetooth;

namespace OpenDataDisc.UI.ViewModels
{
    public class SelectedDeviceViewModel : ViewModelBase
    {
        private readonly BluetoothDevice _device;

        public SelectedDeviceViewModel(BluetoothDevice device)
        {
            _device = device;
        }

        public string Name => _device.Name;

        private string NormalizeIsPaired(bool isPaired) => isPaired ? "Yes" : "No";
        public string IsPaired => $"Paired? {NormalizeIsPaired(_device.IsPaired)}";
    }
}
