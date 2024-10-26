using CommunityToolkit.Maui.Views;
using HeartPlayer.ViewModels;

namespace HeartPlayer.Views
{
    public partial class PlaylistSelectionPopup : Popup
    {
        public PlaylistSelectionPopup(PlaylistSelectionPopupViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        private async void Select_Clicked(object sender, EventArgs e)
        {
            var vm = (PlaylistSelectionPopupViewModel)BindingContext;
            await CloseAsync(vm.SelectedPlaylist);
        }
    }
}
