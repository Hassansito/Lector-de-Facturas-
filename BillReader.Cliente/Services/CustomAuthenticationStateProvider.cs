using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace BillReader.Cliente.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, IHttpClientFactory httpClientFactory)
        {
            _jsRuntime = jsRuntime;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Decodificar el token JWT (sin validar firma, solo para extraer claims)
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            // Opcional: Configurar el token en el HttpClient por defecto para futuras peticiones
            var httpClient = _httpClientFactory.CreateClient("AuthorizedClient");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(user);
        }

        public void MarkUserAsAuthenticated(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void MarkUserAsLoggedOut()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = new List<Claim>();

            foreach (var kvp in keyValuePairs)
            {
                // Reconocer roles: tanto "role" como el URI estándar
                if (kvp.Key == "role" || kvp.Key == ClaimTypes.Role || kvp.Key == "roles")
                {
                    if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in element.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.GetString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()));
                    }
                }
                // Reconocer nombre: "name", "unique_name", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                else if (kvp.Key == "name" || kvp.Key == "unique_name" || kvp.Key == ClaimTypes.Name)
                {
                    claims.Add(new Claim(ClaimTypes.Name, kvp.Value.ToString()));
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
