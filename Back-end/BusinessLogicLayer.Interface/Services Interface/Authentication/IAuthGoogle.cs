namespace BusinessLogicLayer.Interface.Services_Interface.Authentication
{
    public interface IAuthGoogle
    {
        Task<string> AuthenticateGoogleUserAsync(string googleToken);
    }
}
