using OpenDataDisc.UI.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;

namespace OpenDataDisc.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand BuyMusicCommand { get; }
    public ICommand SelectBluetoothDeviceCommand { get; }
    public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }
    public Interaction<BluetoothSelectorViewModel, BluetoothSelectedViewModel?> ShowBluetoothDialog { get; }

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();

    public MainWindowViewModel()
    {
        ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();
        ShowBluetoothDialog = new Interaction<BluetoothSelectorViewModel, BluetoothSelectedViewModel?>();

        BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var store = new MusicStoreViewModel();

            var result = await ShowDialog.Handle(store);

            if (result != null)
            {
                Albums.Add(result);
                await result.SaveToDiskAsync();
            }
        });

        SelectBluetoothDeviceCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var bluetooth = new BluetoothSelectorViewModel();

            var result = await ShowBluetoothDialog.Handle(bluetooth);
        });

        RxApp.MainThreadScheduler.Schedule(LoadAlbums);
    }

    private async void LoadAlbums()
    {
        var albums = (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));

        foreach (var album in albums)
        {
            Albums.Add(album);
        }

        foreach (var album in Albums.ToList())
        {
            await album.LoadCover();
        }
    }
}