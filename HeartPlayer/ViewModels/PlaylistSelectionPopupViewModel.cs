using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using HeartPlayer.Models;

namespace HeartPlayer.ViewModels;

public partial class PlaylistSelectionPopupViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>();

    [ObservableProperty]
    private Playlist _selectedPlaylist;

    public void SetPlaylist(IEnumerable<Playlist> playlists)
    {
        foreach(var playlist in playlists)
        {
            Playlists.Add(playlist);
        }
    }
}
