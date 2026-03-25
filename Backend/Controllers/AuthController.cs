using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VidaAnimal.API.Data;
using VidaAnimal.API.DTOs;
using VidaAnimal.API.Models;
using BCrypt.Net;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == login.Correo && u.Activo == true);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(login.Password, usuario.PasswordHash))
            {
                return Unauthorized(new { success = false, mensaje = "Correo o contraseña incorrectos." });
            }

            // Generar Token JWT
            var tokenString = GenerarJwtToken(usuario);

            return Ok(new 
            { 
                success = true, 
                token = tokenString, 
                usuario = new 
                {
                    nombre = usuario.NombreCompleto,
                    rol = usuario.Rol,
                    correo = usuario.Correo
                }
            });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioCreateDTO registro)
        {
            // Verificar si el correo o DNI ya existen
            if (await _context.Usuarios.AnyAsync(u => u.Correo == registro.Correo))
            {
                return BadRequest(new { success = false, mensaje = "El correo ya está registrado." });
            }
            if (await _context.Usuarios.AnyAsync(u => u.DNI == registro.DNI))
            {
                return BadRequest(new { success = false, mensaje = "El DNI ya está registrado." });
            }

            var nuevoUsuario = new Usuario
            {
                DNI = registro.DNI,
                NombreCompleto = registro.NombreCompleto,
                Correo = registro.Correo,
                // Hasheamos la contraseña con BCrypt
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registro.Password),
                Rol = registro.Rol.ToUpper(), // 'ADMINISTRADOR' o 'CAJERO'
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, mensaje = "Usuario registrado exitosamente." });
        }

        private string GenerarJwtToken(Usuario usuario)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.UsuarioID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim("Nombre", usuario.NombreCompleto),
                // Aquí asignamos el ROL (Crucial para proteger endpoints)
                new Claim(ClaimTypes.Role, usuario.Rol) 
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // El token dura 8 horas
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
