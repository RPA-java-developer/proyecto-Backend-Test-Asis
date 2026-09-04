using ASISYA.Data;
using ASISYA.DTOs.Auth;
using ASISYA.Models;
using ASISYA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASISYA.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDBContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: api/auth/register
        // Nota: en producción, este endpoint debería protegerse o eliminarse
        // una vez creados los usuarios iniciales, para que no cualquiera
        // pueda registrarse libremente.
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto dto)
        {
            bool existe = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (existe)
            {
                return BadRequest("Ese nombre de usuario ya está en uso.");
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, expiresAt) = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            // Mensaje genérico a propósito: no revelar si falló el usuario
            // o la contraseña, para no facilitar enumeración de usuarios.
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            var (token, expiresAt) = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username
            });
        }
    }
}