namespace SetupLayer.Setting.Microservices.Spotify
{
    public class SpotifySettings
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string RedirectUri { get; set; }
    }
}
