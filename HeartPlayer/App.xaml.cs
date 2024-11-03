using HeartPlayer.Services;

namespace HeartPlayer
{
    public partial class App : Application
    {
        private readonly UsageTrackingService _usageTracking;

        public App(UsageTrackingService usageTracking, AppShell appShell)
        {
            InitializeComponent();
            _usageTracking = usageTracking;

            MainPage = appShell;
        }

        protected override void OnStart()
        {
            _usageTracking.StartSession();
        }

        protected override void OnSleep()
        {
            _usageTracking.EndSession();
        }

        protected override void OnResume()
        {
            _usageTracking.StartSession();
        }
    }
}
