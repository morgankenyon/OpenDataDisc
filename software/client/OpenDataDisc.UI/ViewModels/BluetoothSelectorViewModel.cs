﻿using InTheHand.Bluetooth;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;

namespace OpenDataDisc.UI.ViewModels
{
    public class BluetoothSelectorViewModel : ViewModelBase
    {
        private bool _isBusy;
        private CancellationTokenSource? _cancellationTokenSource;

        public BluetoothSelectorViewModel()
        {
            SelectBluetoothDeviceCommand = ReactiveCommand.Create(() =>
            {
                return SelectedDevice;
            });

            RxApp.MainThreadScheduler.Schedule(SearchForDevices);
        }

        public string SearchingForDevicesText
        {
            get => "Searching for Devices";
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

        private async void SearchForDevices()
        {
            IsBusy = true;
            NearbyDevices.Clear();

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            //for some reason these filters are not working as I expect
            var requestOptions = new RequestDeviceOptions();
            var openDataDiscFilter = new BluetoothLEScanFilter
            {
                Name = "OpenDataDisc"
            };
            openDataDiscFilter.Services.Add(Constants.ServiceUuid);
            requestOptions.Filters.Add(openDataDiscFilter);

            var devices = await Bluetooth.ScanForDevicesAsync(
                options: requestOptions,
                cancellationToken: cancellationToken);

            foreach (var device in devices)
            {
                var sdViewModel = new SelectedDeviceViewModel(device);
                NearbyDevices.Add(sdViewModel);
            }

            IsBusy = false;
        }
    }
}
