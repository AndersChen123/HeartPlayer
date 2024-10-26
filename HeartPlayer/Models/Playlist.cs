using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HeartPlayer.Models
{
    public partial class Playlist : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private ObservableCollection<VideoFile> _videos = new ObservableCollection<VideoFile>();
    }
}
