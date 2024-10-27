using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HeartPlayer.Models;
using HeartPlayer.Services;
using HeartPlayer.Views;

namespace HeartPlayer.ViewModels
{
    public partial class PlaylistViewModel : ObservableObject
    {
        private readonly IFileService _fileService;

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

        public PlaylistViewModel(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async void LoadPlaylists()
        {
            var loadedPlaylists = await _fileService.GetPlaylistsAsync();
            Playlists = new ObservableCollection<Playlist>(loadedPlaylists);
        }

        private async Task SavePlaylists()
        {
            await _fileService.SavePlaylistAsync(Playlists.ToList());
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
    }
}
