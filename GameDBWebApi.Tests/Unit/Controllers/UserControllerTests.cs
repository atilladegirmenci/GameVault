using FluentAssertions;
using GameDBWebApi.Controllers;
using GameDBWebApi.DTOs.User;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace GameDBWebApi.Tests.Unit.Controllers;

public class UserControllerTests
{
    [Fact]
    public async Task GetProfileUsesClaimedUser()
    {
        var users = new Mock<IUserService>();
        users.Setup(x => x.GetUserByIdAsync(4)).ReturnsAsync(new UserProfileDto { Id = 4 });
        var controller = new UserController(users.Object) { ControllerContext = Context(4) };

        var result = await controller.GetProfile();

        result.Should().BeOfType<OkObjectResult>();
        users.Verify(x => x.GetUserByIdAsync(4), Times.Once);
    }

    private static ControllerContext Context(int id) =>
        new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test")) } };
}
