using HeartPlayer.ViewModels;

namespace HeartPlayer.Views;

public partial class SettingPage : ContentPage
{
    public SettingPage(SettingViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is SettingViewModel viewModel)
        {
            viewModel.LoadSavedFolders();
        }
    }

    protected override bool OnBackButtonPressed()
    {
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