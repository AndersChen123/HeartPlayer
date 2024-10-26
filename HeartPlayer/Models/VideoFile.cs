using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HeartPlayer.Models
{
    public class VideoFile : ObservableObject
    {
        private ImageSource _thumbnail;

        public int Index { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FolderName { get; set; }

        public DateTime CreationTime { get; set; }

        [JsonIgnore]
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public override bool Equals(object obj)
        {
            return obj is VideoFile file && Path == file.Path;
        }

        public override int GetHashCode()
        {
            return Path?.GetHashCode() ?? 0;
        }
    }
}
