using FluentAssertions;
using GameDBWebApi.Controllers;
using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameDBWebApi.Tests.Unit.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task LoginReturnsOk()
    {
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(new LoginResponseDto { AccessToken = "access" });

        var result = await new AuthController(auth.Object).Login(new LoginRequestDto());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task LogoutWithoutClaimReturnsUnauthorized()
    {
        var controller = new AuthController(Mock.Of<IAuthService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.Logout();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
