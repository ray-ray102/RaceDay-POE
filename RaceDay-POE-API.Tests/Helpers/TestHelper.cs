using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Models;
using RaceDay_POE_API.Services;

namespace RaceDay_POE_API.Tests.Helpers
{
    // Shared setup used by all the test classes
    public static class TestHelper
    {
        // Every test gets its own empty in-memory database
        public static RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        // Gives the controller an HttpContext with a session, optionally logged in
        public static void SetUser(ControllerBase controller, int? userId = null, string? role = null)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (userId != null && role != null)
            {
                httpContext.Session.SetInt32(SessionKeys.UserId, userId.Value);
                httpContext.Session.SetString(SessionKeys.Role, role);
            }

            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        public static User AddUser(RaceDayDbContext context, string email, int roleId)
        {
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = email,
                RoleId = roleId,
                PasswordHash = new PasswordService().HashPassword("Password123")
            };
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        public static Event AddEvent(RaceDayDbContext context, int organiserId)
        {
            var ev = new Event
            {
                OrganiserId = organiserId,
                Name = "Joburg City Run",
                EventDate = DateTime.Today.AddDays(30),
                Location = "Sandton, Johannesburg",
                DistanceKm = 10,
                EventType = "Run"
            };
            context.Events.Add(ev);
            context.SaveChanges();
            return ev;
        }

        public static Category AddCategory(RaceDayDbContext context, int eventId)
        {
            var category = new Category { EventId = eventId, Name = "10km Open", DistanceKm = 10 };
            context.Categories.Add(category);
            context.SaveChanges();
            return category;
        }

        // Pulls the status code out of any IActionResult
        public static int? StatusOf(IActionResult result)
        {
            return result switch
            {
                ObjectResult o => o.StatusCode,
                StatusCodeResult s => s.StatusCode,
                _ => null
            };
        }
    }
}