using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeartPlayer.Views;

namespace HeartPlayer.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToSettings()
    {
        await Shell.Current.GoToAsync(nameof(SettingPage));
        Shell.Current.FlyoutIsPresented = false;
    }

    [RelayCommand]
    private async Task NavigateToAbout()
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
        Shell.Current.FlyoutIsPresented = false;
    }
}
