using Avalonia.ReactiveUI;
using OpenDataDisc.UI.ViewModels;

namespace OpenDataDisc.UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}