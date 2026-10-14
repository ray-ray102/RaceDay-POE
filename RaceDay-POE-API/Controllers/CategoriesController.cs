using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Filters;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Controllers
{
    /*
     Categories always belong to one event.
     Listing and adding go through api/events/{eventId}/categories,
     reading, updating and deleting one category use api/categories/{id}.
    */
    public class CategoriesController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/1/categories
        [HttpGet("api/events/{eventId:int}/categories")]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = "Event not found." });
            }

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(categories.Select(ToCategoryDto).ToList());
        }

        // GET: api/categories/5
        [HttpGet("api/categories/{id:int}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            return Ok(ToCategoryDto(category));
        }

        // POST: api/events/1/categories
        [HttpPost("api/events/{eventId:int}/categories")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCategory(int eventId, CategoryRequestDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (ev.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            if (!IsAgeRangeValid(dto))
            {
                return BadRequest(new { message = "Minimum age cannot be greater than maximum age." });
            }

            var category = new Category
            {
                EventId = eventId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                DistanceKm = dto.DistanceKm
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, ToCategoryDto(category));
        }

        // PUT: api/categories/5
        [HttpPut("api/categories/{id:int}")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, CategoryRequestDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            // Ownership is checked through the event the category belongs to
            if (category.Event!.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            if (!IsAgeRangeValid(dto))
            {
                return BadRequest(new { message = "Minimum age cannot be greater than maximum age." });
            }

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.DistanceKm = dto.DistanceKm;

            await _context.SaveChangesAsync();

            return Ok(ToCategoryDto(category));
        }

        // DELETE: api/categories/5
        [HttpDelete("api/categories/{id:int}")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.Event!.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            // A category with participants in it cannot be removed
            bool hasEnrolments = await _context.Enrolments.AnyAsync(en => en.CategoryId == id);
            if (hasEnrolments)
            {
                return BadRequest(new { message = "This category still has enrolments and cannot be deleted." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Both ages are optional, but when both are given the minimum cannot be bigger
        private static bool IsAgeRangeValid(CategoryRequestDto dto)
        {
            return dto.MinAge == null || dto.MaxAge == null || dto.MinAge <= dto.MaxAge;
        }

        private static CategoryResponseDto ToCategoryDto(Category category)
        {
            return new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                EventId = category.EventId,
                Name = category.Name,
                Description = category.Description,
                MinAge = category.MinAge,
                MaxAge = category.MaxAge,
                DistanceKm = category.DistanceKm
            };
        }
    }
}