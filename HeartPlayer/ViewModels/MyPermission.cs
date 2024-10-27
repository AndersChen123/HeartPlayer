namespace HeartPlayer.ViewModels;

#if ANDROID

public class MyPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
                return new List<(string androidPermission, bool isRuntime)>
                {

                    (Android.Manifest.Permission.ReadMediaVideo, true),
                    (Android.Manifest.Permission.ReadMediaAudio, true),
                    (Android.Manifest.Permission.ReadMediaImages, true)
                }.ToArray();
            }
            else
            {
                return new List<(string androidPermission, bool isRuntime)>
                {
                    (Android.Manifest.Permission.ReadExternalStorage, true),
                    (Android.Manifest.Permission.WriteExternalStorage, true)
                }.ToArray();
            }
#endif
        }
    }
}

public static class PermissionHelper
{
    public static async Task<PermissionStatus> CheckAndRequestStoragePermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<MyPermission>();

        if (status == PermissionStatus.Granted)
            return status;

        if (Permissions.ShouldShowRationale<MyPermission>())
        {
            // Optionally show a rationale for why the permission is needed
            await Shell.Current.DisplayAlert("Permission needed", "This app needs access to your files to play videos.", "OK");
        }

        status = await Permissions.RequestAsync<MyPermission>();

        return status;
    }
}
#endif