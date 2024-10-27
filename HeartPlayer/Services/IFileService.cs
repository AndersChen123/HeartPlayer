using HeartPlayer.Models;

namespace HeartPlayer.Services;

public interface IFileService
{
    Task<List<Playlist>> GetPlaylistsAsync();    

    Task SavePlaylistAsync(List<Playlist> playlists);

    Task<List<Folder>> GetFoldersAsync();

    Task<bool> SaveFolderAsync(List<Folder> folders);
}
