using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeartPlayer.Models;

namespace HeartPlayer.ViewModels
{
    public partial class SettingViewModel : ObservableObject
    {
        public ObservableCollection<Folder> Folders { get; } = new ObservableCollection<Folder>();

        public SettingViewModel()
        {
            LoadSavedFolders();
        }

        private async void LoadSavedFolders()
        {
            try
            {
                var file = Path.Combine(FileSystem.AppDataDirectory, "folders.json");
                if (File.Exists(file))
                {
                    var json = await File.ReadAllTextAsync(file);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var loadedFolders = JsonSerializer.Deserialize<List<Folder>>(json, options);
                    foreach (var folder in loadedFolders)
                    {
                        Folders.Add(folder);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load saved folders: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task PickFolder()
        {
            try
            {
                var result = await FolderPicker.PickAsync(default);
                if (result.IsSuccessful)
                {
                    var folder = new Folder(result.Folder.Name, result.Folder.Path);
                    Folders.Add(folder);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Folder pick failed: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(Folders, options);
                var file = Path.Combine(FileSystem.AppDataDirectory, "folders.json");
                await File.WriteAllTextAsync(file, json);
                await Shell.Current.DisplayAlert("Success", "Folders saved successfully", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save folders: {ex.Message}", "OK");
            }
        }   
    }
}
