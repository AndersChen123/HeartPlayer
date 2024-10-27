using CommunityToolkit.Maui.Views;
using HeartPlayer.Models;

namespace HeartPlayer.Views;

public partial class VideoPlayerPage : ContentPage
{
    private readonly IDispatcherTimer _hideControlsTimer;
    private List<VideoFile> _videos = new List<VideoFile>();
    private int _currentIndex = 0;

    public static readonly BindableProperty MaxVolumeProperty =
        BindableProperty.Create(nameof(MaxVolume), typeof(double), typeof(VideoPlayerPage), 0.8, BindingMode.TwoWay, propertyChanged: MaxVolumePropertyChanged);    

    public static readonly BindableProperty IsVolumeControlVisibleProperty =
        BindableProperty.Create(nameof(IsVolumeControlVisible), typeof(bool), typeof(VideoPlayerPage), false);

    private static void MaxVolumePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var maxVolume = Preferences.Default.Get("MaxVolume", 0.8);
        var page = (VideoPlayerPage)bindable;
        var volume = double.TryParse(newValue.ToString(), out double value) ? value : 0.8;
        if (volume > maxVolume)
        {
            page.MaxVolume = maxVolume;
        }
    }

    public double MaxVolume
    {
        get => (double)GetValue(MaxVolumeProperty);
        set => SetValue(MaxVolumeProperty, value);
    }    

    public bool IsVolumeControlVisible
    {
        get => (bool)GetValue(IsVolumeControlVisibleProperty);
        set => SetValue(IsVolumeControlVisibleProperty, value);
    }

    public VideoPlayerPage(params VideoFile[] videos)
    {
        InitializeComponent();
        BindingContext = this;

        _videos.AddRange(videos);

        Title = _videos[0].Name;
        SetMediaSource(_videos[0].Path);

        MaxVolume = Preferences.Default.Get("MaxVolume", 0.8);

        //var tapGestureRecognizer = new TapGestureRecognizer();
        //tapGestureRecognizer.Tapped += OnScreenTapped;
        //MainGrid.GestureRecognizers.Add(tapGestureRecognizer);

        _hideControlsTimer = Dispatcher.CreateTimer();
        _hideControlsTimer.Interval = TimeSpan.FromSeconds(5);
        _hideControlsTimer.Tick += (s, e) => HideVolumeControl();
    }

    private void SetMediaSource(string videoUrl)
    {
        MediaPlayer.Source = MediaSource.FromFile(videoUrl);        
    }

    protected override bool OnBackButtonPressed()
    {
        return NavigateBack();
    }

    private void OnMediaOpened(object sender, EventArgs e)
    {
        _hideControlsTimer?.Start();
    }

    private void OnMediaEnded(object sender, EventArgs e)
    {
        _currentIndex += 1;
        if (_currentIndex <= _videos.Count - 1)
        {
            var video = _videos[_currentIndex];
            Title = video.Name;
            SetMediaSource(video.Path);
        }
        else
        {
            _currentIndex = 0;
            NavigateBack();
        }
    }

    private void OnMediaFailed(object sender, ErrorEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to play video: {e}", "OK");
            await Navigation.PopAsync();
        });
    }

    private bool NavigateBack()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Navigation.PopAsync();
        });
        return true;
    }

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
        MediaPlayer.Handler?.DisconnectHandler();
    }

    private void OnScreenTapped(object sender, EventArgs e)
    {
        IsVolumeControlVisible = true;
        //_hideControlsTimer.Start();
    }

    private void HideVolumeControl()
    {
        //IsVolumeControlVisible = false;

        var titleBarIsVisible = Shell.GetNavBarIsVisible(this);
        if (titleBarIsVisible)
        {
            Shell.SetNavBarIsVisible(this, false);
        }
        _hideControlsTimer.Stop();
    }    
}
