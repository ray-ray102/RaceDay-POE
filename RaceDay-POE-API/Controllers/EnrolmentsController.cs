using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Filters;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Controllers
{
    // Participants enter events, Organisers manage the enrolments on their own events
    public class EnrolmentsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments
        [HttpPost("api/enrolments")]
        [SessionAuthorize(RoleNames.Participant)]
        [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Enrol(EnrolRequestDto dto)
        {
            var ev = await _context.Events.FindAsync(dto.EventId);
            if (ev == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.EventId != dto.EventId)
            {
                return BadRequest(new { message = "This category does not belong to the selected event." });
            }

            // A participant can only enter an event once
            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(en => en.ParticipantId == CurrentUserId && en.EventId == dto.EventId);
            if (alreadyEnrolled)
            {
                return BadRequest(new { message = "You are already enrolled in this event." });
            }

            var enrolment = new Enrolment
            {
                ParticipantId = CurrentUserId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var created = await LoadEnrolment(enrolment.EnrolmentId);
            return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrolmentId }, ToEnrolmentDto(created!));
        }

        // GET: api/enrolments/mine
        [HttpGet("api/enrolments/mine")]
        [SessionAuthorize(RoleNames.Participant)]
        [ProducesResponseType(typeof(List<EnrolmentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var enrolments = await EnrolmentQuery()
                .Where(en => en.ParticipantId == CurrentUserId)
                .OrderByDescending(en => en.EnrolmentDate)
                .ToListAsync();

            return Ok(enrolments.Select(ToEnrolmentDto).ToList());
        }

        // GET: api/enrolments/5
        [HttpGet("api/enrolments/{id:int}")]
        [SessionAuthorize]
        [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await LoadEnrolment(id);
            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (!CanView(enrolment))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You cannot view this enrolment." });
            }

            return Ok(ToEnrolmentDto(enrolment));
        }

        // GET: api/events/1/enrolments
        [HttpGet("api/events/{eventId:int}/enrolments")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(List<EnrolmentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
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

            var enrolments = await EnrolmentQuery()
                .Where(en => en.EventId == eventId)
                .OrderBy(en => en.EnrolmentDate)
                .ToListAsync();

            return Ok(enrolments.Select(ToEnrolmentDto).ToList());
        }

        // PUT: api/enrolments/5/status
        [HttpPut("api/enrolments/{id:int}/status")]
        [SessionAuthorize]
        [ProducesResponseType(typeof(EnrolmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, EnrolmentStatusDto dto)
        {
            var enrolment = await LoadEnrolment(id);
            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            /*
             The owning Organiser can set Confirmed or Cancelled.
             A Participant can only cancel their own enrolment.
            */
            bool isOwnerOrganiser = CurrentRole == RoleNames.Organiser && enrolment.Event!.OrganiserId == CurrentUserId;
            bool isOwnerParticipant = CurrentRole == RoleNames.Participant && enrolment.ParticipantId == CurrentUserId;

            bool allowed = (isOwnerOrganiser && dto.Status != "Pending")
                || (isOwnerParticipant && dto.Status == "Cancelled");

            if (!allowed)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You cannot change this enrolment to that status." });
            }

            enrolment.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(ToEnrolmentDto(enrolment));
        }

        // DELETE: api/enrolments/5
        [HttpDelete("api/enrolments/{id:int}")]
        [SessionAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await LoadEnrolment(id);
            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (!CanView(enrolment))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You cannot delete this enrolment." });
            }

            // Any result linked to this enrolment is removed by the cascade delete
            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // The participant who owns it, or the organiser of the event, may access an enrolment
        private bool CanView(Enrolment enrolment)
        {
            if (CurrentRole == RoleNames.Participant)
            {
                return enrolment.ParticipantId == CurrentUserId;
            }

            return CurrentRole == RoleNames.Organiser && enrolment.Event!.OrganiserId == CurrentUserId;
        }

        private IQueryable<Enrolment> EnrolmentQuery()
        {
            return _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.Event)
                .Include(en => en.Category);
        }

        private Task<Enrolment?> LoadEnrolment(int id)
        {
            return EnrolmentQuery().FirstOrDefaultAsync(en => en.EnrolmentId == id);
        }

        private static EnrolmentResponseDto ToEnrolmentDto(Enrolment enrolment)
        {
            return new EnrolmentResponseDto
            {
                EnrolmentId = enrolment.EnrolmentId,
                ParticipantId = enrolment.ParticipantId,
                ParticipantName = enrolment.Participant == null ? string.Empty : enrolment.Participant.FirstName + " " + enrolment.Participant.LastName,
                EventId = enrolment.EventId,
                EventName = enrolment.Event?.Name ?? string.Empty,
                CategoryId = enrolment.CategoryId,
                CategoryName = enrolment.Category?.Name ?? string.Empty,
                EnrolmentDate = enrolment.EnrolmentDate,
                Status = enrolment.Status
            };
        }
    }
}