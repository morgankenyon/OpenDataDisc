using ReactiveUI;
using System.Reactive.Linq;
using System.Windows.Input;

namespace OpenDataDisc.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand SelectBluetoothDeviceCommand { get; }
    public Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?> ShowBluetoothDialog { get; }
    
    private SelectedDeviceViewModel? _selectedDevice;
    public SelectedDeviceViewModel? SelectedDevice
    {
        get => _selectedDevice;
        set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
    }

    public MainWindowViewModel()
    {
        ShowBluetoothDialog = new Interaction<BluetoothSelectorViewModel, SelectedDeviceViewModel?>();

        SelectBluetoothDeviceCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var bluetooth = new BluetoothSelectorViewModel();

            var result = await ShowBluetoothDialog.Handle(bluetooth);

            if (result != null)
            {
                SelectedDevice = result;
            }
        });
    }
}