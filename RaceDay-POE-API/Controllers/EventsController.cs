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