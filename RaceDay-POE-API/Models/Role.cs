using System.ComponentModel.DataAnnotations;

namespace RaceDay_POE_API.Models
{
    // Lookup table for the two roles in the system, Organiser and Participant
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        // All users that have this role
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}