using BillReader.Cliente.Services.Interfaces;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BillReader.Cliente.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJSRuntime _jsRuntime;

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetUserNameAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userName") ?? "";
        }

        public async Task<string> GetUserRoleAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userRole") ?? "";
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userName");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userRole");
        }
    }
}
