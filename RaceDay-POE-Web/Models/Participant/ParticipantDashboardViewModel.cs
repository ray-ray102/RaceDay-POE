using RaceDay_POE_Web.Models.Api;

namespace RaceDay_POE_Web.Models.Participant;

public class ParticipantDashboardViewModel
{
    public List<EnrolmentResponseDto> Enrolments { get; set; } = new();
    public List<ResultDetailDto> Results { get; set; } = new();
}
