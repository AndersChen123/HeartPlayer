using HeartPlayer.ViewModels;
using HeartPlayer.Views;

namespace HeartPlayer
{
    public partial class AppShell : Shell
    {
        public AppShell(AppShellViewModel viewModel)
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(VideoPlayerPage), typeof(VideoPlayerPage));
            Routing.RegisterRoute(nameof(SettingPage), typeof(SettingPage));
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));

            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is AppShellViewModel viewModel)
            {
                viewModel.StartCheckingUsageTime();
            }
        }
    }
}
