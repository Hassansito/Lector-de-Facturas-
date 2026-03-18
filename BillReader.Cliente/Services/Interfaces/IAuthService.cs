namespace BillReader.Cliente.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetUserNameAsync();
        Task<string> GetUserRoleAsync();
        Task LogoutAsync();
    }
}
