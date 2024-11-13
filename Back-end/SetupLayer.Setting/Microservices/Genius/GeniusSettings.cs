namespace SetupLayer.Setting.Microservices.Genius
{
    public class GeniusSettings
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string RedirectUri { get; set; }
        public required string State { get; set; }
    }
}
