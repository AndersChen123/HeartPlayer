namespace HeartPlayer.Models
{
    internal class PlayerConfig
    {
        public PlayerConfig()
        {
            Folders = new List<string>();
        }

        public List<string> Folders { get; set; }
    }
}
