using FluentAssertions;
using GameDBWebApi.Controllers;
using GameDBWebApi.DTOs;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace GameDBWebApi.Tests.Unit.Controllers;

public class GameControllerTests
{
    [Fact]
    public async Task SearchRejectsBlankKeyword()
    {
        var result = await new GameController(Mock.Of<IGameService>())
            .SearchGameByTitle(" ", 1, 10, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUserGamesClampsPageSize()
    {
        var game = new Mock<IGameService>();
        game.Setup(x => x.GetUserGamesAsync(4, 1, 40, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedList<GameCardDto>([], 0, 1, 40));
        var controller = new GameController(game.Object) { ControllerContext = Context(4) };

        await controller.GetUserGames(1, 100, CancellationToken.None);

        game.Verify(x => x.GetUserGamesAsync(4, 1, 40, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ControllerContext Context(int id) =>
        new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test")) } };
}
