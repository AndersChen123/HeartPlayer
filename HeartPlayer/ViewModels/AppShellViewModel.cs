using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeartPlayer.Services;
using HeartPlayer.Views;

namespace HeartPlayer.ViewModels;

public partial class AppShellViewModel : ObservableObject
{    
    private readonly UsageTrackingService _usageTrackingService;
    private IDispatcherTimer _timer;

    public AppShellViewModel(UsageTrackingService usageTrackingService)
    {
        _usageTrackingService = usageTrackingService;        
    }

    public void StartCheckingUsageTime()
    {
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(10);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

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

    private async void Timer_Tick(object sender, EventArgs e)
    {
        var usageDuration = Preferences.Default.Get("UsageDuration", 60);
        var totalUsageTime = _usageTrackingService.GetTotalUsageTime();

        if (totalUsageTime.TotalMinutes >= usageDuration)
        {
            await Shell.Current.DisplayAlert("Reminder", "You've been looking at your phone for too long, do something more fun.", "OK");
            Application.Current.Quit();
        }
    }
}
