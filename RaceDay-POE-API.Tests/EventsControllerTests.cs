using RaceDay_POE_API.Controllers;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Tests.Helpers;

namespace RaceDay_POE_API.Tests
{
    public class EventsControllerTests
    {
        private static EventRequestDto NewEvent()
        {
            return new EventRequestDto
            {
                Name = "Durban Beachfront Walk",
                EventDate = DateTime.Today.AddDays(14),
                Location = "North Beach, Durban",
                DistanceKm = 5,
                EventType = "Walk"
            };
        }

        [Fact]
        public async Task CreateEvent_UsesOrganiserFromSession()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, organiser.UserId, RoleNames.Organiser);

            var result = await controller.CreateEvent(NewEvent());

            Assert.Equal(201, TestHelper.StatusOf(result));
            Assert.Equal(organiser.UserId, context.Events.Single().OrganiserId);
        }

        [Fact]
        public async Task GetEvent_UnknownId_Returns404()
        {
            var context = TestHelper.CreateContext();
            var controller = new EventsController(context);
            TestHelper.SetUser(controller);

            var result = await controller.GetEvent(999);

            Assert.Equal(404, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task GetEvents_FiltersByEventType()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            TestHelper.AddEvent(context, organiser.UserId);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, organiser.UserId, RoleNames.Organiser);
            await controller.CreateEvent(NewEvent());

            var result = await controller.GetEvents("Walk", null, null);

            var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
            var events = Assert.IsType<List<EventResponseDto>>(ok.Value);
            Assert.Single(events);
            Assert.Equal("Walk", events[0].EventType);
        }

        [Fact]
        public async Task UpdateEvent_NotOwner_Returns403()
        {
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", 1);
            var other = TestHelper.AddUser(context, "other@test.com", 1);
            var ev = TestHelper.AddEvent(context, owner.UserId);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, other.UserId, RoleNames.Organiser);

            var result = await controller.UpdateEvent(ev.EventId, NewEvent());

            Assert.Equal(403, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task DeleteEvent_Owner_Returns204()
        {
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", 1);
            var ev = TestHelper.AddEvent(context, owner.UserId);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, owner.UserId, RoleNames.Organiser);

            var result = await controller.DeleteEvent(ev.EventId);

            Assert.Equal(204, TestHelper.StatusOf(result));
            Assert.Empty(context.Events);
        }

        [Fact]
        public async Task DeleteEvent_WithEnrolments_Returns400()
        {
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", 1);
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var ev = TestHelper.AddEvent(context, owner.UserId);
            var category = TestHelper.AddCategory(context, ev.EventId);
            context.Enrolments.Add(new Models.Enrolment { ParticipantId = participant.UserId, EventId = ev.EventId, CategoryId = category.CategoryId });
            context.SaveChanges();
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, owner.UserId, RoleNames.Organiser);

            var result = await controller.DeleteEvent(ev.EventId);

            Assert.Equal(400, TestHelper.StatusOf(result));
        }
    }
}