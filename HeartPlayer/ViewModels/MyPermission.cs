namespace HeartPlayer.ViewModels;

#if ANDROID

public class MyPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
            {
                (Android.Manifest.Permission.ReadMediaVideo, true),
                (Android.Manifest.Permission.ReadMediaAudio, true),
                (Android.Manifest.Permission.ReadMediaImages, true)
            }.ToArray();
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