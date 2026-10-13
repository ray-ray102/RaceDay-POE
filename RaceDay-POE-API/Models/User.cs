using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_POE_API.Models
{
    // Stores both Organisers and Participants, the role is chosen at registration
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Only the hashed password is stored, never the plain text password
        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        // Events this user created (only used when the user is an Organiser)
        public ICollection<Event> Events { get; set; } = new List<Event>();

        // Enrolments this user made (only used when the user is a Participant)
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}