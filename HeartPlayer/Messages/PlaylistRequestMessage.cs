using CommunityToolkit.Mvvm.Messaging.Messages;
using HeartPlayer.Models;

namespace HeartPlayer.Messages;

public class PlaylistRequestMessage : RequestMessage<List<Playlist>>
{
}
