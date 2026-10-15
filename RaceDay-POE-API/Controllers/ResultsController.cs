using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Filters;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Controllers
{
    // Organisers capture results for their events, anyone can view an event leaderboard
    public class ResultsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments/5/results
        [HttpPost("api/enrolments/{enrolmentId:int}/results")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(ResultResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CaptureResult(int enrolmentId, ResultRequestDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(en => en.Event)
                .Include(en => en.Result)
                .FirstOrDefaultAsync(en => en.EnrolmentId == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (enrolment.Event!.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            // Only confirmed entries can get a result, and only one result each
            if (enrolment.Status != "Confirmed")
            {
                return BadRequest(new { message = "Results can only be captured for confirmed enrolments." });
            }

            if (enrolment.Result != null)
            {
                return BadRequest(new { message = "This enrolment already has a result." });
            }

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = dto.FinishTime,
                FinishPosition = dto.FinishPosition
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResultForEnrolment), new { enrolmentId = enrolmentId }, ToResultDto(result));
        }

        // GET: api/enrolments/5/results
        [HttpGet("api/enrolments/{enrolmentId:int}/results")]
        [SessionAuthorize]
        [ProducesResponseType(typeof(ResultResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetResultForEnrolment(int enrolmentId)
        {
            var enrolment = await _context.Enrolments
                .Include(en => en.Event)
                .Include(en => en.Result)
                .FirstOrDefaultAsync(en => en.EnrolmentId == enrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            // The participant who entered or the organiser of the event can see it
            bool isParticipant = CurrentRole == RoleNames.Participant && enrolment.ParticipantId == CurrentUserId;
            bool isOrganiser = CurrentRole == RoleNames.Organiser && enrolment.Event!.OrganiserId == CurrentUserId;

            if (!isParticipant && !isOrganiser)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You cannot view this result." });
            }

            if (enrolment.Result == null)
            {
                return NotFound(new { message = "No result has been captured yet." });
            }

            return Ok(ToResultDto(enrolment.Result));
        }

        // GET: api/events/1/results
        [HttpGet("api/events/{eventId:int}/results")]
        [ProducesResponseType(typeof(List<ResultDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
            {
                return NotFound(new { message = "Event not found." });
            }

            var results = await ResultQuery()
                .Where(r => r.Enrolment!.EventId == eventId)
                .OrderBy(r => r.FinishPosition)
                .ToListAsync();

            return Ok(results.Select(ToDetailDto).ToList());
        }

        // GET: api/results/mine
        [HttpGet("api/results/mine")]
        [SessionAuthorize(RoleNames.Participant)]
        [ProducesResponseType(typeof(List<ResultDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await ResultQuery()
                .Where(r => r.Enrolment!.ParticipantId == CurrentUserId)
                .OrderByDescending(r => r.Enrolment!.Event!.EventDate)
                .ToListAsync();

            return Ok(results.Select(ToDetailDto).ToList());
        }

        // PUT: api/results/5
        [HttpPut("api/results/{id:int}")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(typeof(ResultResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateResult(int id, ResultRequestDto dto)
        {
            var result = await ResultQuery().FirstOrDefaultAsync(r => r.ResultsId == id);
            if (result == null)
            {
                return NotFound(new { message = "Result not found." });
            }

            if (result.Enrolment!.Event!.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            result.FinishTime = dto.FinishTime;
            result.FinishPosition = dto.FinishPosition;
            await _context.SaveChangesAsync();

            return Ok(ToResultDto(result));
        }

        // DELETE: api/results/5
        [HttpDelete("api/results/{id:int}")]
        [SessionAuthorize(RoleNames.Organiser)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await ResultQuery().FirstOrDefaultAsync(r => r.ResultsId == id);
            if (result == null)
            {
                return NotFound(new { message = "Result not found." });
            }

            if (result.Enrolment!.Event!.OrganiserId != CurrentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IQueryable<Result> ResultQuery()
        {
            return _context.Results
                .Include(r => r.Enrolment).ThenInclude(en => en!.Event)
                .Include(r => r.Enrolment).ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment).ThenInclude(en => en!.Category);
        }

        private static ResultResponseDto ToResultDto(Result result)
        {
            return new ResultResponseDto
            {
                ResultsId = result.ResultsId,
                EnrolmentId = result.EnrolmentId,
                FinishTime = result.FinishTime,
                FinishPosition = result.FinishPosition
            };
        }

        private static ResultDetailDto ToDetailDto(Result result)
        {
            var enrolment = result.Enrolment!;
            return new ResultDetailDto
            {
                ResultsId = result.ResultsId,
                EnrolmentId = result.EnrolmentId,
                FinishTime = result.FinishTime,
                FinishPosition = result.FinishPosition,
                EventId = enrolment.EventId,
                EventName = enrolment.Event?.Name ?? string.Empty,
                EventDate = enrolment.Event?.EventDate ?? DateTime.MinValue,
                ParticipantName = enrolment.Participant == null ? string.Empty : enrolment.Participant.FirstName + " " + enrolment.Participant.LastName,
                CategoryName = enrolment.Category?.Name ?? string.Empty
            };
        }
    }
}