using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Filters;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Controllers
{
    // Profile endpoints for the logged in user, both roles use the same routes
    [Route("api/users")]
    [SessionAuthorize]
    public class UsersController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/users/me
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            // The session points to a user that no longer exists
            if (user == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            return Ok(ToProfileDto(user));
        }

        // PUT: api/users/me
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber;
            user.ProfilePictureUrl = dto.ProfilePictureUrl;

            await _context.SaveChangesAsync();

            return Ok(ToProfileDto(user));
        }

        // Turns a User into the profile shape the client sees
        private static UserProfileDto ToProfileDto(User user)
        {
            return new UserProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role?.RoleName ?? string.Empty
            };
        }
    }
}