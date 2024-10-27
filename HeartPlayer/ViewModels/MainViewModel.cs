using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HeartPlayer.Messages;
using HeartPlayer.Models;
using HeartPlayer.Services;
using HeartPlayer.Views;
using Serilog;
using Folder = HeartPlayer.Models.Folder;

namespace HeartPlayer.ViewModels
{
    public partial class MainViewModel : ObservableRecipient
    {
        private readonly ThumbnailService _thumbnailService;
        private readonly IPopupService _popupService;
        private readonly IFileService _fileService;
        private readonly PlaylistViewModel _playlistViewModel;

        private bool _isLoaded = false;

        [ObservableProperty]
        private bool _isShowingVideos = true;

        [ObservableProperty]
        private Folder _selectedFolder;

        [ObservableProperty]
        private ObservableCollection<Folder> _folders;

        [ObservableProperty]
        private ObservableCollection<VideoFile> _videos;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private KeyValuePair<string, Enum> _selectedSortOption;

        public enum SortOption
        {
            NameAscending,
            NameDescending,
            CreationTimeAscending,
            CreationTimeDescending
        }

        [ObservableProperty]
        private ObservableCollection<VideoFile> _selectedVideos = new ObservableCollection<VideoFile>();

        [ObservableProperty]
        private bool _isSelectionModeActive = false;

        public MainViewModel(ThumbnailService thumbnailService, IPopupService popupService, IFileService fileService)
        {
            _thumbnailService = thumbnailService;
            _popupService = popupService;
            _fileService = fileService;

            Videos = new ObservableCollection<VideoFile>();
            Folders = new ObservableCollection<Folder>();
            SelectedSortOption = new KeyValuePair<string, Enum>("Newest First", SortOption.CreationTimeDescending);

            IsShowingVideos = Preferences.Get("IsShowingVideos", true);

            InitializeAsync();
        }

        public async void InitializeAsync()
        {
            if (_isLoaded) return;
            _isLoaded = true;

            Messenger.Register<LoadVideoMessage>(this, async (recipient, message) =>
            {
                var folder = new Folder(message.FolderName, message.Path);
                Folders.Add(folder);
                await LoadVideosForFolder(folder);
            });

            await LoadFolders();
            await LoadVideos();
        }

        private bool IsVideoFile(string filePath)
        {
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };
            return videoExtensions.Contains(Path.GetExtension(filePath).ToLowerInvariant());
        }

        private async Task LoadFolders()
        {
            try
            {
                var loadedFolders = await _fileService.GetFoldersAsync();
                Folders = new ObservableCollection<Folder>(loadedFolders);

                //foreach (var folder in Folders)
                //{
                //    folder.LoadVideos();
                //    folder.Thumbnail = await _thumbnailService.GetFolderThumbnailAsync(folder);
                //}
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading folders");
                await Shell.Current.DisplayAlert("Error", $"Error loading folders: {ex.Message}", "OK");
            }
        }

        private async Task LoadVideos()
        {
#if ANDROID
            var status = await PermissionHelper.CheckAndRequestStoragePermission();

            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permission Denied", "Cannot access videos without storage permission.", "OK");
                return;
            }
#endif

            IsLoading = true;
            try
            {
                Videos.Clear();
                foreach (var folder in Folders)
                {
                    await LoadVideosForFolder(folder);                    
                }

                SortVideos(); // Apply sorting after loading videos
                IsLoading = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load videos");
                await Shell.Current.DisplayAlert("Error", "Failed to load videos", "OK");
            }
        }

        private async Task LoadVideosForFolder(Folder folder)
        {
            try
            {
                IsLoading = true;
                // For iOS, request permission for this specific folder
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.StorageRead>();
                        if (status != PermissionStatus.Granted)
                        {
                            await Shell.Current.DisplayAlert("Permission Denied", $"Cannot access folder: {folder.Name}", "OK");
                            return;
                        }
                    }
                }

                var videoFiles = Directory.EnumerateFiles(folder.Path, "*.*", SearchOption.AllDirectories)
                                          .Where(IsVideoFile)
                                          .Select(file => new VideoFile
                                          {
                                              Name = Path.GetFileName(file),
                                              Path = file,
                                              FolderName = folder.Name,
                                              CreationTime = File.GetCreationTime(file)
                                          });

                foreach (var video in videoFiles)
                {
                    folder.AddVideo(video);
                    Videos.Add(video);
                }

                folder.Thumbnail = await _thumbnailService.GetFolderThumbnailAsync(folder);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error loading videos for folder {folder.Name}");
                await Shell.Current.DisplayAlert("Error", $"Error loading videos for folder {folder.Name}: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void OpenFolder(Folder folder)
        {
            SelectedFolder = folder;
            folder.LoadVideos();
            Videos = new ObservableCollection<VideoFile>(folder.Videos);
            IsShowingVideos = true;
        }

        [RelayCommand]
        private async Task PlayVideo(VideoFile video)
        {
            await Shell.Current.Navigation.PushAsync(new VideoPlayerPage(video));
        }

        [RelayCommand]
        private async Task ToggleView()
        {
            IsShowingVideos = !IsShowingVideos;
            if (IsShowingVideos)
            {
                await LoadVideos();
            }
        }

        private void SortVideos()
        {
            if (Enum.TryParse(SelectedSortOption.Value.ToString(), out SortOption order))
            {
                var sortedVideos = order switch
                {
                    SortOption.NameAscending => Videos.OrderBy(v => v.Name),
                    SortOption.NameDescending => Videos.OrderByDescending(v => v.Name),
                    SortOption.CreationTimeAscending => Videos.OrderBy(v => v.CreationTime),
                    SortOption.CreationTimeDescending => Videos.OrderByDescending(v => v.CreationTime),
                    _ => Videos.OrderBy(v => v.Name)
                };

                var newList = sortedVideos.ToList();
                Videos.Clear();
                foreach (var video in newList)
                {
                    Videos.Add(video);
                }
            }
        }

        partial void OnSelectedSortOptionChanged(KeyValuePair<string, Enum> value)
        {
            SortVideos();
        }

        [RelayCommand]
        private void ToggleVideoSelection(VideoFile video)
        {
            if (SelectedVideos.Contains(video))
            {
                SelectedVideos.Remove(video);
            }
            else
            {
                SelectedVideos.Add(video);
            }
        }

        [RelayCommand]
        private async Task AddSelectedVideosToPlaylist()
        {
            if (SelectedVideos.Count > 0)
            {
                try
                {
                    var playlists = await _fileService.GetPlaylistsAsync();

                    var result = await _popupService.ShowPopupAsync<PlaylistSelectionPopupViewModel>(onPresenting: viewModel => viewModel.SetPlaylist(playlists));
                    if (result != null && result is Playlist playlist)
                    {
                        var item = playlists.FirstOrDefault(p => p.Name == playlist.Name);
                        if (item != null)
                        {
                            foreach (var video in item.Videos)
                            {
                                if (!item.Videos.Contains(video))
                                {
                                    item.Videos.Add(video);
                                }
                            }
                            await _fileService.SavePlaylistAsync(playlists);
                        }

                        await Shell.Current.DisplayAlert("Success", $"Added {SelectedVideos.Count} videos to {playlist.Name}", "OK");

                        ToggleSelectionMode();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error adding selected videos to playlist");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Please select videos to add", "OK");
            }
        }

        [RelayCommand]
        private void ToggleSelectionMode()
        {
            IsSelectionModeActive = !IsSelectionModeActive;
            if (!IsSelectionModeActive)
            {
                SelectedVideos.Clear();
            }
        }
    }
}
