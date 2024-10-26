namespace HeartPlayer.Views;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
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