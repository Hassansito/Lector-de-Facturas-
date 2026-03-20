using BillReader.Cliente.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BillReader.Cliente.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(IJSRuntime jsRuntime, AuthenticationStateProvider authStateProvider)
        {
            _jsRuntime = jsRuntime;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetUserNameAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public async Task<string> GetUserRoleAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            
        }

        public async Task LogoutAsync()
        {         
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.MarkUserAsLoggedOut();
            }
        }
    }
}