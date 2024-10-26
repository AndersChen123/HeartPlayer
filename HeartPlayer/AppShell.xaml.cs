using HeartPlayer.Views;

namespace HeartPlayer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(VideoPlayerPage), typeof(VideoPlayerPage));
            Routing.RegisterRoute(nameof(SettingPage), typeof(SettingPage));
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        }
    }
}
