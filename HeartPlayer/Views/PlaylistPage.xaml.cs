using HeartPlayer.ViewModels;

namespace HeartPlayer.Views
{
    public partial class PlaylistPage : ContentPage
    {
        public PlaylistPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            if (BindingContext is PlaylistViewModel viewModel)
            {
                if (viewModel.IsShowingVideos)
                {
                    viewModel.IsShowingVideos = false;
                    return true;
                }
            }

            return NavigateBack();
        }

        private bool NavigateBack()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
            });
            return true;
        }
    }
}
