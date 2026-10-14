using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Filters;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Controllers
{
    // Browse events publicly, Organisers can manage the events they own
    [Route("api/events")]
    public class EventsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events?eventType=Run&location=Durban&fromDate=2026-09-01
        [HttpGet]
        [ProducesResponseType(typeof(List<EventResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents(string? eventType, string? location, DateTime? fromDate)
        {
            // Only upcoming events are listed unless a start date is given
            DateTime startDate = fromDate ?? DateTime.Today;

            var query = _context.Events
                .Include(e => e.Categories)
                .Where(e => e.EventDate >= startDate);

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(e => e.EventType == eventType);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(e => e.Location.Contains(location));
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();

            return Ok(events.Select(ToEventDto).ToList());
        }

        // GET: api/events/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(ToEventDto(ev));
        }

        // GET: api/events/mine
        [HttpGet("mine")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(List<OrganiserEventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEvents()
        {
            var events = await _context.Events
                .Where(e => e.OrganiserId == CurrentUserId)
                .OrderBy(e => e.EventDate)
                .Select(e => new OrganiserEventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    EventType = e.EventType,
                    EnrolmentCount = e.Enrolments.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/events
        [HttpPost]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent(EventRequestDto dto)
        {
            // The owner always comes from the session, never from the request body
            var ev = new Event
            {
                OrganiserId = CurrentUserId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location.Trim(),
                DistanceKm = dto.DistanceKm,
                EventType = dto.EventType,
                BannerImageUrl = dto.BannerImageUrl,
                CreatedAt = DateTime.Now
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, ToEventDto(ev));
        }

        // PUT: api/events/5
        [HttpPut("{id:int}")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(EventResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, EventRequestDto dto)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            // Organisers can only edit their own events
            if (ev.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            ev.Name = dto.Name.Trim();
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location.Trim();
            ev.DistanceKm = dto.DistanceKm;
            ev.EventType = dto.EventType;
            ev.BannerImageUrl = dto.BannerImageUrl;

            await _context.SaveChangesAsync();

            return Ok(ToEventDto(ev));
        }

        // DELETE: api/events/5
        [HttpDelete("{id:int}")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (ev.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            // The database will not allow an event with enrolments to be deleted
            bool hasEnrolments = await _context.Enrolments.AnyAsync(en => en.EventId == id);
            if (hasEnrolments)
            {
                return BadRequest(new { message = "This event still has enrolments and cannot be deleted." });
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Turns an Event into the shape the client sees
        private static EventResponseDto ToEventDto(Event ev)
        {
            return new EventResponseDto
            {
                EventId = ev.EventId,
                OrganiserId = ev.OrganiserId,
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                DistanceKm = ev.DistanceKm,
                EventType = ev.EventType,
                BannerImageUrl = ev.BannerImageUrl,
                Categories = ev.Categories.Select(c => new CategorySummaryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    DistanceKm = c.DistanceKm
                }).ToList()
            };
        }
    }
}