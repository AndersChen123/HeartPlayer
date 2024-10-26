using System.Collections.ObjectModel;

namespace HeartPlayer.Models
{
    public class Folder : IEquatable<Folder>
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ObservableCollection<VideoFile> Videos { get; set; }
        public ImageSource Thumbnail { get; set; }

        public Folder(string name, string path)
        {
            Name = name;
            Path = path;
            Videos = new ObservableCollection<VideoFile>();
        }

        public void LoadVideos()
        {
            try
            {
                Videos.Clear();
                var videoFiles = Directory.GetFiles(Path, "*.*", SearchOption.AllDirectories)
                                          .Where(file => IsVideoFile(file))
                                          .Select(file => new VideoFile
                                          {
                                              Name = System.IO.Path.GetFileName(file),
                                              Path = file,
                                              FolderName = Name,
                                              CreationTime = File.GetCreationTime(file)
                                          });

                foreach (var video in videoFiles)
                {
                    Videos.Add(video);
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"Error loading videos for folder {Name}: {ex.Message}");
            }
        }

        private bool IsVideoFile(string filePath)
        {
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };
            return videoExtensions.Contains(System.IO.Path.GetExtension(filePath).ToLowerInvariant());
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Folder);
        }

        public bool Equals(Folder other)
        {
            return other != null && Path == other.Path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(Folder left, Folder right)
        {
            return EqualityComparer<Folder>.Default.Equals(left, right);
        }

        public static bool operator !=(Folder left, Folder right)
        {
            return !(left == right);
        }
    }
}
