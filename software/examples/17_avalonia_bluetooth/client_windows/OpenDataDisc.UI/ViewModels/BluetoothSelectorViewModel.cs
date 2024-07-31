using InTheHand.Bluetooth;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace OpenDataDisc.UI.ViewModels
{
    public class BluetoothSelectorViewModel : ViewModelBase
    {
        private SelectedDeviceViewModel? _selectedBluetoothDevice;
        public SelectedDeviceViewModel? SelectedBluetoothDevice
        {
            get => _selectedBluetoothDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedBluetoothDevice, value);
        }

        private string? _searchText;
        private bool _isBusy;
        private CancellationTokenSource? _cancellationTokenSource;

        public BluetoothSelectorViewModel()
        {
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(2000))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SearchForDevices);

            SelectBluetoothDeviceCommand = ReactiveCommand.Create(() =>
            {
                return SelectedBluetoothDevice;
            });
        }

        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public ReactiveCommand<Unit, SelectedDeviceViewModel?> SelectBluetoothDeviceCommand { get; }

        private SelectedDeviceViewModel? _selectedDevice;

        public SelectedDeviceViewModel? SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }
        public ObservableCollection<SelectedDeviceViewModel> NearbyDevices { get; } = new();

        private async void SearchForDevices(string? s)
        {
            IsBusy = true;
            NearbyDevices.Clear();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var devices = await Bluetooth.ScanForDevicesAsync(cancellationToken: cancellationToken);

            foreach (var device in devices)
            {
                var sdViewModel = new SelectedDeviceViewModel(device);
                NearbyDevices.Add(sdViewModel);
            }

            IsBusy = false;
        }
    }
}
