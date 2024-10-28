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

        public void HandleMessage(object? sender, GattCharacteristicValueChangedEventArgs e)
        {
            MessageCount++;
        }
    }
}
