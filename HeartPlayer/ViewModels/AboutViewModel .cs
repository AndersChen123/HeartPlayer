using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HeartPlayer.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [RelayCommand]
    private async Task OpenWebsite()
    {
        try
        {
            Uri uri = new Uri("https://heartplayer.app");
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.External);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Could not open website", "OK");
        }
    }

    public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
}
