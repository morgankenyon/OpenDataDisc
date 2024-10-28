using InTheHand.Bluetooth;
using ReactiveUI;

namespace OpenDataDisc.UI.ViewModels
{
    public class ConfigurationWindowViewModel : ViewModelBase
    {
        public ConfigurationWindowViewModel() { }

        private int _messageCount;
        public int MessageCount
        {
            get => _messageCount;
            set => this.RaiseAndSetIfChanged(ref _messageCount, value);
        }

        private ConfigurationStep _step;
        public ConfigurationStep Step
        {
            get => _step;
            set => this.RaiseAndSetIfChanged(ref _step, value);
        }

        public void HandleMessage(object? sender, GattCharacteristicValueChangedEventArgs e)
        {
            MessageCount++;
        }
    }

    public enum ConfigurationStep
    {
        Start,
        AccXSetup,
        AccXRecording,
        AccYSetup,
        AccYRecording,
        AccZSetup,
        AccZRecording,
        GyroSetup,
        GyroRecording,
    }
}
