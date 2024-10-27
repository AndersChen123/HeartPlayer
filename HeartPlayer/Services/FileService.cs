using HeartPlayer.Models;
using HeartPlayer.Utils;
using Serilog;

namespace HeartPlayer.Services;

public class FileService : IFileService
{
    private const string PlaylistFileName = "playlists.json";
    private const string FolderFileName = "folders.json";

    public async Task<List<Folder>> GetFoldersAsync()
    {
        try
        {
            var file = Path.Combine(FileSystem.AppDataDirectory, FolderFileName);
            if (File.Exists(file))
            {
                var loadedFolders = await FileHelper.ReadFromJson<List<Folder>>(file);
                return loadedFolders;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load saved folders");
        }

        return new List<Folder>();
    }

    public async Task<List<Playlist>> GetPlaylistsAsync()
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, PlaylistFileName);
            if (File.Exists(filePath))
            {
                var loadedPlaylists = await FileHelper.ReadFromJson<List<Playlist>>(filePath);
                return loadedPlaylists;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error loading playlists");
        }

        return new List<Playlist>();
    }

    public async Task<bool> SaveFolderAsync(List<Folder> folders)
    {
        try
        {
            var file = Path.Combine(FileSystem.AppDataDirectory, FolderFileName);
            await FileHelper.SaveAsJson(folders, file);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save folders");
        }

        return false;
    }

    public async Task SavePlaylistAsync(List<Playlist> playlists)
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, PlaylistFileName);
            await FileHelper.SaveAsJson(playlists, filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error saving playlists");
        }
    }
}
