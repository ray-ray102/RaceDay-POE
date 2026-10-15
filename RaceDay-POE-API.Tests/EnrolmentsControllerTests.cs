using RaceDay_POE_API.Controllers;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Tests.Helpers;

namespace RaceDay_POE_API.Tests
{
    public class EnrolmentsControllerTests
    {
        [Fact]
        public async Task Enrol_ValidRequest_Returns201WithPendingStatus()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var category = TestHelper.AddCategory(context, ev.EventId);
            var controller = new EnrolmentsController(context);
            TestHelper.SetUser(controller, participant.UserId, RoleNames.Participant);

            var result = await controller.Enrol(new EnrolRequestDto { EventId = ev.EventId, CategoryId = category.CategoryId });

            Assert.Equal(201, TestHelper.StatusOf(result));
            Assert.Equal("Pending", context.Enrolments.Single().Status);
        }

        [Fact]
        public async Task Enrol_SameEventTwice_Returns400()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var category = TestHelper.AddCategory(context, ev.EventId);
            var controller = new EnrolmentsController(context);
            TestHelper.SetUser(controller, participant.UserId, RoleNames.Participant);
            var dto = new EnrolRequestDto { EventId = ev.EventId, CategoryId = category.CategoryId };

            await controller.Enrol(dto);
            var result = await controller.Enrol(dto);

            Assert.Equal(400, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task Enrol_CategoryFromAnotherEvent_Returns400()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var otherEvent = TestHelper.AddEvent(context, organiser.UserId);
            var otherCategory = TestHelper.AddCategory(context, otherEvent.EventId);
            var controller = new EnrolmentsController(context);
            TestHelper.SetUser(controller, participant.UserId, RoleNames.Participant);

            var result = await controller.Enrol(new EnrolRequestDto { EventId = ev.EventId, CategoryId = otherCategory.CategoryId });

            Assert.Equal(400, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task Enrol_UnknownEvent_Returns404()
        {
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var controller = new EnrolmentsController(context);
            TestHelper.SetUser(controller, participant.UserId, RoleNames.Participant);

            var result = await controller.Enrol(new EnrolRequestDto { EventId = 999, CategoryId = 1 });

            Assert.Equal(404, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task UpdateStatus_ParticipantTriesToConfirm_Returns403()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var category = TestHelper.AddCategory(context, ev.EventId);
            var controller = new EnrolmentsController(context);
            TestHelper.SetUser(controller, participant.UserId, RoleNames.Participant);
            await controller.Enrol(new EnrolRequestDto { EventId = ev.EventId, CategoryId = category.CategoryId });
            int enrolmentId = context.Enrolments.Single().EnrolmentId;

            var result = await controller.UpdateStatus(enrolmentId, new EnrolmentStatusDto { Status = "Confirmed" });

            Assert.Equal(403, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task UpdateStatus_OwnerOrganiserConfirms_Returns200()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var category = TestHelper.AddCategory(context, ev.EventId);
            context.Enrolments.Add(new Models.Enrolment { ParticipantId = participant.UserId, EventId = ev.EventId, CategoryId = category.CategoryId });
            context.SaveChanges();
            var controller = new EnrolmentsController(context);
            TestHelper.SetUser(controller, organiser.UserId, RoleNames.Organiser);

            var result = await controller.UpdateStatus(context.Enrolments.Single().EnrolmentId, new EnrolmentStatusDto { Status = "Confirmed" });

            Assert.Equal(200, TestHelper.StatusOf(result));
            Assert.Equal("Confirmed", context.Enrolments.Single().Status);
        }
    }
}