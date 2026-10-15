using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay_POE_API.Filters;
using RaceDay_POE_API.Helpers;
using RaceDay_POE_API.Tests.Helpers;

namespace RaceDay_POE_API.Tests
{
    // Checks that the SessionAuthorize filter rejects the wrong users
    public class RoleAuthorizationTests
    {
        private static AuthorizationFilterContext BuildContext(int? userId, string? role)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (userId != null && role != null)
            {
                httpContext.Session.SetInt32(SessionKeys.UserId, userId.Value);
                httpContext.Session.SetString(SessionKeys.Role, role);
            }

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void NoSession_Returns401()
        {
            var filter = new SessionAuthorizeAttribute(RoleNames.Organiser);
            var context = BuildContext(null, null);

            filter.OnAuthorization(context);

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void ParticipantOnOrganiserEndpoint_Returns403()
        {
            var filter = new SessionAuthorizeAttribute(RoleNames.Organiser);
            var context = BuildContext(3, RoleNames.Participant);

            filter.OnAuthorization(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void OrganiserOnParticipantEndpoint_Returns403()
        {
            var filter = new SessionAuthorizeAttribute(RoleNames.Participant);
            var context = BuildContext(1, RoleNames.Organiser);

            filter.OnAuthorization(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void CorrectRole_IsAllowedThrough()
        {
            var filter = new SessionAuthorizeAttribute(RoleNames.Organiser);
            var context = BuildContext(1, RoleNames.Organiser);

            filter.OnAuthorization(context);

            // No result means the request carries on to the action
            Assert.Null(context.Result);
        }

        [Fact]
        public void AnyLoggedInUser_IsAllowedWhenNoRoleIsGiven()
        {
            var filter = new SessionAuthorizeAttribute();
            var context = BuildContext(3, RoleNames.Participant);

            filter.OnAuthorization(context);

            Assert.Null(context.Result);
        }
    }
}