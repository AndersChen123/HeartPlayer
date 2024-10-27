using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog;
using HeartPlayer.Services;
using HeartPlayer.ViewModels;
using HeartPlayer.Views;
using LibVLCSharp.Shared;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace HeartPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                // Initialize the .NET MAUI Community Toolkit by adding the below line of code
                .UseMauiCommunityToolkit()
                // Initialize the .NET MAUI Community Toolkit MediaElement by adding the below line of code
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            Core.Initialize();

            builder.Services.AddSingleton<ThumbnailService>();
            builder.Services.AddSingleton<IFileService, FileService>();

            builder.Services.AddScoped<MainViewModel>();
            builder.Services.AddScoped<PlaylistViewModel>();  
            builder.Services.AddScoped<SettingViewModel>();
            builder.Services.AddScoped<AboutViewModel>();

            builder.Services.AddScoped<MainPage>();
            builder.Services.AddScoped<PlaylistPage>();
            builder.Services.AddScoped<SettingPage>();
            builder.Services.AddScoped<AboutPage>();
            builder.Services.AddScoped<VideoPlayerPage>();

            builder.Services.AddTransientPopup<PlaylistSelectionPopup, PlaylistSelectionPopupViewModel>();

            Ioc.Default.ConfigureServices(builder.Services.BuildServiceProvider());

            SetupSerilog();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void SetupSerilog()
        {
            var flushInterval = new TimeSpan(0, 1, 0);
            var file = Path.Combine(FileSystem.AppDataDirectory, "Logs", "HeartPlayer.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(file, flushToDiskInterval: flushInterval, encoding: System.Text.Encoding.UTF8,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 2)
                .CreateLogger();
        }
    }
}
