namespace BillReader.Controllers;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Models.Entities;
using Services.Services;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(ApplicationDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    // POST: api/auth/registro
    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroDto registroDto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == registroDto.NombreUsuario))
            return BadRequest("El nombre de usuario ya existe.");

        registroDto.NombreUsuario = registroDto.NombreUsuario.Trim();
        registroDto.Password = registroDto.Password.Trim();

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NombreUsuario = registroDto.NombreUsuario,
            Rol = registroDto.Rol,
            PasswordHash = _passwordHasher.HashPassword(registroDto.Password)
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Usuario creado exitosamente" });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == loginDto.NombreUsuario);
        if (usuario == null)
            return Unauthorized("Credenciales inválidas.");

        if (!_passwordHasher.VerifyPassword(loginDto.Password, usuario.PasswordHash))
            return Unauthorized("Credenciales inválidas.");

        var token = _tokenService.GenerarToken(usuario);
        Console.WriteLine($"Hash almacenado: '{usuario.PasswordHash}' (longitud: {usuario.PasswordHash?.Length})");
        Console.WriteLine($"Contraseña ingresada: '{loginDto.Password}'");
        bool verifica = _passwordHasher.VerifyPassword(loginDto.Password, usuario.PasswordHash);
        Console.WriteLine($"Verificación: {verifica}");
        return Ok(new TokenResponseDto { Token = token,Rol = usuario.Rol,NombreUsuario = usuario.NombreUsuario });
    }
}