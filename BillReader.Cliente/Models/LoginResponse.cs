using System.Text.Json.Serialization;

namespace BillReader.Cliente.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = "";

        [JsonPropertyName("nombreUsuario")]
        public string Nombre { get; set; } = "";

        [JsonPropertyName("rol")]
        public string Rol { get; set; } = "";
    }
}
