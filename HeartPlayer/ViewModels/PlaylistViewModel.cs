using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HeartPlayer.Messages;
using HeartPlayer.Models;
using HeartPlayer.Views;
using Microsoft.Maui.Controls;
using System.IO;

namespace HeartPlayer.ViewModels
{
    public partial class PlaylistViewModel : ObservableRecipient
    {
        private const string PlaylistFileName = "playlists.json";

        [ObservableProperty]
        private ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>();

        [ObservableProperty]
        private ObservableCollection<VideoFile> _playlistVideos = new ObservableCollection<VideoFile>();

        [ObservableProperty]
        private bool _isShowingVideos;
        
        [ObservableProperty]
        private Playlist selectedPlaylist;

        [ObservableProperty]
        private string newPlaylistName;

        public PlaylistViewModel()
        {
            Messenger.Register<PlaylistViewModel, PlaylistRequestMessage>(this, (r, m) =>
            {
                m.Reply(r.Playlists.ToList());
            });
            Messenger.Register<PlaylistViewModel, AddVideoToPlaylistMessage>(this, (r, m) =>
            {
                _ = r.AddVideosToPlaylist(m);
            }); 

            LoadPlaylists();
        }        

        private async void LoadPlaylists()
        {
            try
            {
                string filePath = Path.Combine(FileSystem.AppDataDirectory, PlaylistFileName);
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    var loadedPlaylists = JsonSerializer.Deserialize<List<Playlist>>(json);
                    Playlists = new ObservableCollection<Playlist>(loadedPlaylists);
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"Error loading playlists: {ex.Message}");
            }
        }

        private async Task SavePlaylists()
        {
            try
            {
                string filePath = Path.Combine(FileSystem.AppDataDirectory, PlaylistFileName);
                string json = JsonSerializer.Serialize(Playlists.ToList());
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"Error saving playlists: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task CreatePlaylist()
        {
            if (!string.IsNullOrWhiteSpace(NewPlaylistName))
            {
                Playlists.Add(new Playlist { Name = NewPlaylistName });
                NewPlaylistName = string.Empty;
                await SavePlaylists();
            }
        }

        [RelayCommand]
        private async Task DeletePlaylist(Playlist playlist)
        {
            if (playlist != null)
            {
                Playlists.Remove(playlist);
                await SavePlaylists();
            }
        }

        [RelayCommand]
        private async Task AddVideoToPlaylist(VideoFile video)
        {
            if (SelectedPlaylist != null && video != null)
            {
                SelectedPlaylist.Videos.Add(video);
                await SavePlaylists();
            }
        }

        [RelayCommand]
        private async Task RemoveVideoFromPlaylist(VideoFile video)
        {
            if (SelectedPlaylist != null && video != null)
            {
                SelectedPlaylist.Videos.Remove(video);
                await SavePlaylists();
            }
        }

        [RelayCommand]
        private void OpenPlaylist(Playlist value)
        {
            if (value != null)
            {
                PlaylistVideos.Clear();
                foreach (var item in value.Videos)
                {
                    PlaylistVideos.Add(item);
                }

                IsShowingVideos = true;
            }
            else
            {
                PlaylistVideos.Clear();
            }
        }

        [RelayCommand]
        private async Task PlayVideo(VideoFile video)
        {
            await Shell.Current.Navigation.PushAsync(new VideoPlayerPage(video));
        }

        [RelayCommand]
        private async Task PlayPlaylist(Playlist playlist)
        {
            await Shell.Current.Navigation.PushAsync(new VideoPlayerPage(playlist.Videos.ToArray()));
        }

        private async Task AddVideosToPlaylist(AddVideoToPlaylistMessage m)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Name == m.Playlist);
            if (playlist != null)
            {
                foreach (var video in m.Videos)
                {
                    if (!playlist.Videos.Contains(video))
                    {
                        playlist.Videos.Add(video);
                    }
                }
                await SavePlaylists();
            }
        }
    }
}
