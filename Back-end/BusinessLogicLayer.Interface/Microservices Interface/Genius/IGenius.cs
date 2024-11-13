namespace BusinessLogicLayer.Interface.Microservices_Interface.Genius
{
    public interface IGenius
    {
        string Authorize();
        Task<string> GetAccessToken(string authorizationCode);
        Task<string?> GetUrlLyricsAsync(string accessToken, string trackName, string artistName);
    }
}
