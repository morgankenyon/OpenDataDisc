using ReactiveUI;
using System.Reactive;

namespace OpenDataDisc.UI.ViewModels
{
    public class BluetoothSelectorViewModel : ViewModelBase
    {
        private BluetoothSelectedViewModel? _selectedBluetoothDevice;
        public BluetoothSelectedViewModel? SelectedBluetoothDevice
        {
            get => _selectedBluetoothDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedBluetoothDevice, value);
        }

        public BluetoothSelectorViewModel()
        {
            SelectBluetoothDeviceCommand = ReactiveCommand.Create(() =>
            {
                return SelectedBluetoothDevice;
            });
        }

        public ReactiveCommand<Unit, BluetoothSelectedViewModel?> SelectBluetoothDeviceCommand { get; }
    }
}
