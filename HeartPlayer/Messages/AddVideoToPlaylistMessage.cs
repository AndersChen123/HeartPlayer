using HeartPlayer.Models;

namespace HeartPlayer.Messages;

internal class AddVideoToPlaylistMessage
{
    public string Playlist {get;set;}

    public List<VideoFile> Videos { get; set; }
}
