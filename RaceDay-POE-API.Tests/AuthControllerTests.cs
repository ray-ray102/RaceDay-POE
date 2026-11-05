using RaceDay_POE_API.Controllers;
using RaceDay_POE_API.DTOs;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Services;
using RaceDay_POE_API.Tests.Helpers;
using Microsoft.AspNetCore.Http;

namespace RaceDay_POE_API.Tests
{
    public class AuthControllerTests
    {
        private static RegisterDto NewRegister(string email = "sipho@test.com", string role = "Participant")
        {
            return new RegisterDto
            {
                FirstName = "Sipho",
                LastName = "Dlamini",
                Email = email,
                Password = "MySecurePassword123",
                Role = role
            };
        }

        [Fact]
        public async Task Register_ValidDetails_Returns201AndHashesPassword()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, new PasswordService());
            TestHelper.SetUser(controller);

            var result = await controller.Register(NewRegister());

            Assert.Equal(201, TestHelper.StatusOf(result));
            var saved = context.Users.Single();
            Assert.NotEqual("MySecurePassword123", saved.PasswordHash);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, new PasswordService());
            TestHelper.SetUser(controller);

            await controller.Register(NewRegister());
            var result = await controller.Register(NewRegister());

            Assert.Equal(400, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, new PasswordService());
            TestHelper.SetUser(controller);

            var result = await controller.Register(NewRegister(role: "Admin"));

            Assert.Equal(400, TestHelper.StatusOf(result));
        }

        [Fact]
        public async Task Login_CorrectPassword_StoresUserInSession()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, new PasswordService());
            TestHelper.SetUser(controller);
            await controller.Register(NewRegister());

            var result = await controller.Login(new LoginDto { Email = "sipho@test.com", Password = "MySecurePassword123" });

            Assert.Equal(200, TestHelper.StatusOf(result));
            var session = controller.HttpContext.Session;
            Assert.NotNull(session.GetInt32(SessionKeys.UserId));
            Assert.Equal("Participant", session.GetString(SessionKeys.Role));
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, new PasswordService());
            TestHelper.SetUser(controller);
            await controller.Register(NewRegister());

            var result = await controller.Login(new LoginDto { Email = "sipho@test.com", Password = "WrongPassword" });

            Assert.Equal(401, TestHelper.StatusOf(result));
        }

        [Fact]
        public void Logout_ClearsSession()
        {
            var context = TestHelper.CreateContext();
            var controller = new AuthController(context, new PasswordService());
            TestHelper.SetUser(controller, 1, RoleNames.Participant);

            var result = controller.Logout();

            Assert.Equal(200, TestHelper.StatusOf(result));
            Assert.Null(controller.HttpContext.Session.GetInt32(SessionKeys.UserId));
        }
    }
}