using HeartPlayer.Models;
using HeartPlayer.ViewModels;

namespace HeartPlayer.Views
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        private void OnVideoSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is VideoFile selectedVideo)
            {
                // Deselect the item
                ((CollectionView)sender).SelectedItem = null;

                // Play the video
                (BindingContext as MainViewModel)?.PlayVideoCommand.Execute(selectedVideo);
            }
        }

        private void OnVideoCheckBoxChanged(object sender, CheckedChangedEventArgs e)
        {
            if (BindingContext is MainViewModel viewModel && sender is CheckBox checkBox && checkBox.BindingContext is VideoFile video)
            {
                viewModel.ToggleVideoSelectionCommand.Execute(video);
            }
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (BindingContext is MainViewModel viewModel)
            {
                viewModel.InitializeAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            var viewModel = (MainViewModel)BindingContext;
            if (viewModel.IsSelectionModeActive)
            {
                viewModel.ToggleSelectionModeCommand.Execute(null);
                return true;
            }

            if (viewModel.IsShowingVideos == false)
            {
                viewModel.ToggleViewCommand.Execute(null);
                return true;
            }
            else if (viewModel.IsShowingVideos && viewModel.SelectedFolder != null)
            {
                viewModel.SelectedFolder = null;
                viewModel.ToggleViewCommand.Execute(null);
                return true;
            }
            return base.OnBackButtonPressed();
        }
    }
}
