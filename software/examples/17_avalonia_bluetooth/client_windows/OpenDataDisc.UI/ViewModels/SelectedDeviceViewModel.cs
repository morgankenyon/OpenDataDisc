using InTheHand.Bluetooth;
using System.Windows.Navigation;

namespace OpenDataDisc.UI.ViewModels
{
    public class SelectedDeviceViewModel : ViewModelBase
    {
        private BluetoothDevice _device;

        public BluetoothDevice Device { 
            get =>  _device; 
            set => _device = value;
        }

        public SelectedDeviceViewModel(BluetoothDevice device)
        {
            _device = device;
        }

        public string Name => _device.Name;

        private string NormalizeIsPaired(bool isPaired) => isPaired ? "Yes" : "No";
        public string IsPaired => $"Paired? {NormalizeIsPaired(_device.IsPaired)}";
    }
}
