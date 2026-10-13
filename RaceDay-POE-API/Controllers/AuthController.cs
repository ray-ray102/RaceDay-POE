using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Models;
using RaceDay_POE_API.Services;

namespace RaceDay_POE_API.Controllers
{
    // Handles registering, logging in and logging out
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IPasswordService _passwordService;

        public AuthController(RaceDayDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // The role has to be one of the two roles in the Roles table
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null)
            {
                return BadRequest(new { message = "Role must be Organiser or Participant." });
            }

            string email = dto.Email.Trim().ToLower();
            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailTaken)
            {
                return BadRequest(new { message = "This email is already in use." });
            }

            var user = new User
            {
                RoleId = role.RoleId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = role.RoleName
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            string email = dto.Email.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Same message for a wrong email or a wrong password so we do not leak which one it was
            if (user == null || !_passwordService.VerifyPassword(user.PasswordHash, dto.Password))
            {
                return Unauthorized(new { message = "Email or password is incorrect." });
            }

            // Save who the user is in the session
            HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
            HttpContext.Session.SetString(SessionKeys.Role, user.Role!.RoleName);

            return Ok(new AuthResponseDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.RoleName
            });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            if (HttpContext.Session.GetInt32(SessionKeys.UserId) == null)
            {
                return Unauthorized(new { message = "You are not logged in." });
            }

            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }
}