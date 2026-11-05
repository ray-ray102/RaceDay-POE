using Microsoft.AspNetCore.Http;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Tests.Helpers;

namespace RaceDay_POE_API.Tests
{
    public class UploadControllerTests
    {
        private static IFormFile MakeJpeg(string name = "banner.jpg", int bytes = 128)
        {
            var stream = new MemoryStream(Enumerable.Repeat((byte)1, bytes).ToArray());
            return new FormFile(stream, 0, bytes, "file", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        [Fact]
        public async Task UploadBanner_OwnerOrganiser_StoresUrl()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var controller = TestHelper.CreateEventsController(context);
            TestHelper.SetUser(controller, organiser.UserId, RoleNames.Organiser);

            var result = await controller.UploadBanner(ev.EventId, MakeJpeg());

            Assert.Equal(200, TestHelper.StatusOf(result));
            Assert.False(string.IsNullOrWhiteSpace(context.Events.Single().BannerImageUrl));
            Assert.Contains("raceday-banners", context.Events.Single().BannerImageUrl!);
        }

        [Fact]
        public async Task UploadBanner_OtherOrganiser_Returns403()
        {
            var context = TestHelper.CreateContext();
            var owner = TestHelper.AddUser(context, "owner@test.com", 1);
            var other = TestHelper.AddUser(context, "other@test.com", 1);
            var ev = TestHelper.AddEvent(context, owner.UserId);
            var controller = TestHelper.CreateEventsController(context);
            TestHelper.SetUser(controller, other.UserId, RoleNames.Organiser);

            var result = await controller.UploadBanner(ev.EventId, MakeJpeg());

            Assert.Equal(403, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task UploadBanner_InvalidFile_Returns400()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddUser(context, "org@test.com", 1);
            var ev = TestHelper.AddEvent(context, organiser.UserId);
            var controller = TestHelper.CreateEventsController(context);
            TestHelper.SetUser(controller, organiser.UserId, RoleNames.Organiser);
            var bad = new FormFile(new MemoryStream(new byte[10]), 0, 10, "file", "x.gif")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/gif"
            };

            var result = await controller.UploadBanner(ev.EventId, bad);

            Assert.Equal(400, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task UploadProfilePicture_Participant_StoresUrl()
        {
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddUser(context, "par@test.com", 2);
            var controller = TestHelper.CreateUsersController(context);
            TestHelper.SetUser(controller, participant.UserId, RoleNames.Participant);

            var result = await controller.UploadProfilePicture(MakeJpeg("avatar.jpg"));

            Assert.Equal(200, TestHelper.StatusOf(result));
            Assert.False(string.IsNullOrWhiteSpace(context.Users.Single(u => u.UserId == participant.UserId).ProfilePictureUrl));
            Assert.Contains("raceday-profiles", context.Users.Single(u => u.UserId == participant.UserId).ProfilePictureUrl!);
        }
    }
}
