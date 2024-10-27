using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HeartPlayer.Messages;
using HeartPlayer.Models;
using HeartPlayer.Services;
using Serilog;

namespace HeartPlayer.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        private readonly IFileService _fileService;

        public ObservableCollection<Folder> Folders { get; } = new ObservableCollection<Folder>();

        public SettingViewModel(IFileService fileService)
        {
            _fileService = fileService;

            MaxVolume = Preferences.Default.Get("MaxVolume", 0.8);
            IsShowingVideos = Preferences.Default.Get("IsShowingVideos", true);
        }

        public async void LoadSavedFolders()
        {
            Folders.Clear();
            var loadedFolders = await _fileService.GetFoldersAsync();
            foreach (var folder in loadedFolders)
            {
                Folders.Add(folder);
            }
        }

        [ObservableProperty]
        private double _maxVolume = 0.8;

        [ObservableProperty]
        private bool _isShowingVideos = true;

        [RelayCommand]
        private async Task PickFolder()
        {
            try
            {
                var result = await FolderPicker.PickAsync(default);
                if (result.IsSuccessful)
                {
                    var folder = new Folder(result.Folder.Name, result.Folder.Path);
                    if (!Folders.Contains(folder))
                    {
                        Folders.Add(folder);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Error", "Folder already exists", "OK");
                        return;
                    }

                    WeakReferenceMessenger.Default.Send(new LoadVideoMessage
                    {
                        FolderName = folder.Name,
                        Path = folder.Path
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Folder pick failed");
                await Shell.Current.DisplayAlert("Error", $"Folder pick failed: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private void DeleteFolder(Folder folder)
        {
            Folders.Remove(folder);
        }

        [RelayCommand]
        private async Task Save()
        {
            Preferences.Default.Set("MaxVolume", MaxVolume);
            Preferences.Default.Set("IsShowingVideos", IsShowingVideos);

            if (!Folders.Any()) return;

            var ret = await _fileService.SaveFolderAsync(Folders.ToList());

            if (ret)
            {
                await Shell.Current.DisplayAlert("Success", "Folders saved successfully", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save folders", "OK");
            }
        }
    }
}
