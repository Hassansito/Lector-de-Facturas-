using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BillReader.Cliente.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [JsonPropertyName("nombreUsuario")] 
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [JsonPropertyName("password")] 
        public string Password { get; set; } = "";
    }
}
