using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Data
{
    // The database context for RaceDay, one DbSet for each table in the ERD
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role names must be unique
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            // Seed the two roles so they exist as soon as the database is created
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Organiser" },
                new Role { RoleId = 2, RoleName = "Participant" });

            // No two users can share an email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event rules, only Run, Walk or Cycle and a positive distance
            modelBuilder.Entity<Event>(e =>
            {
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Events_EventType", "EventType IN ('Run','Walk','Cycle')");
                    t.HasCheckConstraint("CK_Events_DistanceKm", "DistanceKm IS NULL OR DistanceKm > 0");
                });

                e.Property(ev => ev.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.HasIndex(ev => ev.EventDate);

                e.HasOne(ev => ev.Organiser)
                    .WithMany(u => u.Events)
                    .HasForeignKey(ev => ev.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Categories are removed when their event is removed
            modelBuilder.Entity<Category>(c =>
            {
                c.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Categories_AgeRange", "MinAge IS NULL OR MaxAge IS NULL OR MinAge <= MaxAge");
                    t.HasCheckConstraint("CK_Categories_DistanceKm", "DistanceKm IS NULL OR DistanceKm > 0");
                });

                c.HasOne(cat => cat.Event)
                    .WithMany(ev => ev.Categories)
                    .HasForeignKey(cat => cat.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            /*
             A participant can only enrol once per event.
             Restrict is used on the links so an event or category
             with enrolments cannot be deleted by accident.
            */
            modelBuilder.Entity<Enrolment>(en =>
            {
                en.ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status", "Status IN ('Pending','Confirmed','Cancelled')"));

                en.HasIndex(x => new { x.ParticipantId, x.EventId }).IsUnique();
                en.HasIndex(x => x.Status);

                en.Property(x => x.EnrolmentDate).HasDefaultValueSql("GETDATE()");
                en.Property(x => x.Status).HasDefaultValue("Pending");

                en.HasOne(x => x.Participant)
                    .WithMany(u => u.Enrolments)
                    .HasForeignKey(x => x.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                en.HasOne(x => x.Event)
                    .WithMany(ev => ev.Enrolments)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                en.HasOne(x => x.Category)
                    .WithMany(c => c.Enrolments)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // One result per enrolment, deleted along with the enrolment
            modelBuilder.Entity<Result>(r =>
            {
                r.ToTable(t => t.HasCheckConstraint("CK_Results_FinishPosition", "FinishPosition > 0"));

                r.HasIndex(x => x.EnrolmentId).IsUnique();

                r.HasOne(x => x.Enrolment)
                    .WithOne(en => en.Result)
                    .HasForeignKey<Result>(x => x.EnrolmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}