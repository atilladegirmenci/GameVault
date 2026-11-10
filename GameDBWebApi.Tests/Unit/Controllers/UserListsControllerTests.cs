using FluentAssertions;
using GameDBWebApi.Controllers;
using GameDBWebApi.DTOs.List;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace GameDBWebApi.Tests.Unit.Controllers;

public class UserListsControllerTests
{
    [Fact]
    public async Task CreateListReturnsCreated()
    {
        var lists = new Mock<IUserListService>();
        lists.Setup(x => x.CreateListAsync(4, It.IsAny<CreateUserListRequestDto>()))
            .ReturnsAsync(new UserListResponseDto { Id = 8 });
        var controller = new UserListsController(lists.Object) { ControllerContext = Context(4) };

        var result = await controller.CreateList(new CreateUserListRequestDto { ListTitle = "Favorites" });

        result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)result).StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    private static ControllerContext Context(int id) =>
        new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test")) } };
}
