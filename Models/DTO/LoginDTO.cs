namespace Models.DTO
{
    public class RegistroDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class TokenResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
