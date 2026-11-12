using FluentAssertions;
using GameDBWebApi.DTOs;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;
using GameDBWebApi.Repositories;
using GameDBWebApi.Services;
using Moq;

namespace GameDBWebApi.Tests;

public class GameServiceTests
{
    [Fact]
    public async Task UpsertGameStatusAsync_AddsCatalogGameAndUserGame()
    {
        var games = new Mock<IGameRepository>();
        var userGames = new Mock<IUserGameRepository>();
        var catalog = new Mock<IGameCatalogService>();
        var unitOfWork = TestDoubles.UnitOfWork(games: games, userGames: userGames);
        games.Setup(x => x.GetByCatalogIdAsync(42)).ReturnsAsync((Game?)null);
        catalog.Setup(x => x.GetGameByCatalogIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Game { CatalogId = 42, Title = "Game", ImageUrl = "image" });
        games.Setup(x => x.AddAsync(It.IsAny<Game>())).Callback<Game>(x => x.Id = 7);
        userGames.Setup(x => x.GetByUserIDAndGameIdAsync(5, 7)).ReturnsAsync((UserGame?)null);
        var service = new GameService(unitOfWork.Object, catalog.Object, TestDoubles.Logger<GameService>());

        await service.UpsertGameStatusAsync(5, new UpsertGameStatusDto { CatalogId = 42 }, CancellationToken.None);

        games.Verify(x => x.AddAsync(It.Is<Game>(g => g.CatalogId == 42)), Times.Once);
        userGames.Verify(x => x.AddAsync(It.Is<UserGame>(g => g.UserId == 5 && g.GameId == 7)), Times.Once);
        unitOfWork.Verify(x => x.CompleteAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task GetGameDetailsAsync_AuthenticatedUserAddsLibraryStatus()
    {
        var games = new Mock<IGameRepository>();
        var userGames = new Mock<IUserGameRepository>();
        var catalog = new Mock<IGameCatalogService>();
        var details = new GameDetailsResponseDto { CatalogId = 42, Title = "Game" };
        catalog.Setup(x => x.GetGameDetailsByCatalogIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(details);
        games.Setup(x => x.GetByCatalogIdAsync(42)).ReturnsAsync(new Game { Id = 7, CatalogId = 42 });
        userGames.Setup(x => x.GetByUserIDAndGameIdAsync(5, 7))
            .ReturnsAsync(new UserGame { UserId = 5, GameId = 7, Status = GameStatus.Playing });
        var service = new GameService(
            TestDoubles.UnitOfWork(games: games, userGames: userGames).Object,
            catalog.Object,
            TestDoubles.Logger<GameService>());

        var result = await service.GetGameDetailsAsync(5, 42, CancellationToken.None);

        result.IsInMyLibrary.Should().BeTrue();
        result.MyStatus.Should().Be(GameStatus.Playing);
        userGames.Verify(x => x.GetByUserIDAndGameIdAsync(5, 7), Times.Once);
    }

    [Fact]
    public async Task GetUserGamesAsync_WhenEmptyReturnsEmptyPage()
    {
        var userGames = new Mock<IUserGameRepository>();
        userGames.Setup(x => x.GetByUserIdAsync(5, 1, 10))
            .ReturnsAsync(new PaginatedList<UserGame>([], 0, 1, 10));
        var service = new GameService(
            TestDoubles.UnitOfWork(userGames: userGames).Object,
            Mock.Of<IGameCatalogService>(),
            TestDoubles.Logger<GameService>());

        var result = await service.GetUserGamesAsync(5, 1, 10, CancellationToken.None);

        result.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchGamesAsync_UsesNewestOrderingAndAddsLibraryStatus()
    {
        var catalog = new Mock<IGameCatalogService>();
        var userGames = new Mock<IUserGameRepository>();
        var expected = new PaginatedFeedResponse
        {
            CurrentPage = 1,
            Games =
            [
                new GameCardDto { CatalogId = 42, Title = "Zelda", ImageUrl = "image" }
            ]
        };
        catalog.Setup(x => x.GetGamesByCatalogAsync(1, 10, "zelda", "-added", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        userGames.Setup(x => x.GetStatusesByCatalogIdsAsync(
                5,
                It.Is<List<int>>(ids => ids.SequenceEqual(new List<int> { 42 }))))
            .ReturnsAsync(
            [
                new UserGame
                {
                    GameId = 7,
                    Status = GameStatus.Playing,
                    UpdatedDate = new DateTime(2026, 11, 12),
                    Game = new Game { CatalogId = 42 }
                }
            ]);
        var service = new GameService(
            TestDoubles.UnitOfWork(userGames: userGames).Object,
            catalog.Object,
            TestDoubles.Logger<GameService>());

        var result = await service.SearchGamesAsync(5, 1, 10, "zelda", CancellationToken.None);

        result.Should().BeSameAs(expected);
        result.Games[0].GameId.Should().Be(7);
        result.Games[0].Status.Should().Be(GameStatus.Playing);
        result.Games[0].UpdatedDate.Should().Be(new DateTime(2026, 11, 12));
    }
}
