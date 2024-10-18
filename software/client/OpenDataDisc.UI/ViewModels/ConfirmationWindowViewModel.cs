using OpenDataDisc.UI.Models;
using ReactiveUI;
using System.Reactive;

namespace OpenDataDisc.UI.ViewModels
{
    public class ConfirmationWindowViewModel : ViewModelBase
    {
        public ConfirmationWindowViewModel(string confirmationMessage)
        {
            ConfirmationMessage = confirmationMessage;

            TransmitConfirmationResultCommand = ReactiveCommand.Create(() =>
            {
                return ConfirmationResult;
            });
        }
        public ReactiveCommand<Unit, ConfirmationResult> TransmitConfirmationResultCommand { get; }

        public string ConfirmationMessage { get; }
        private ConfirmationResult _confirmationResult;

        public ConfirmationResult ConfirmationResult
        {
            get => _confirmationResult;
            set => this.RaiseAndSetIfChanged(ref _confirmationResult, value);
        }
    }
}
