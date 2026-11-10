using FluentAssertions;
using GameDBWebApi.DTOs.List;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;
using GameDBWebApi.Services;
using Moq;

namespace GameDBWebApi.Tests;

public class UserListServiceTests
{
    [Fact]
    public async Task CreateListAsync_TrimsTitleAndReturnsCreatedList()
    {
        var lists = new Mock<IUserListRepository>();
        lists.Setup(x => x.AddAsync(It.IsAny<UserList>())).Callback<UserList>(x => x.Id = 4);
        var service = new UserListService(TestDoubles.UnitOfWork(userLists: lists).Object, Mock.Of<IGameCatalogService>());

        var result = await service.CreateListAsync(2, new CreateUserListRequestDto { ListTitle = "  Favorites  " });

        result.Id.Should().Be(4);
        result.ListTitle.Should().Be("Favorites");
        result.GameCount.Should().Be(0);
    }

    [Fact]
    public async Task UpdateListAsync_RejectsBlankTitle()
    {
        var service = new UserListService(TestDoubles.UnitOfWork().Object, Mock.Of<IGameCatalogService>());

        await service.Invoking(x => x.UpdateListAsync(2, 4, new UpdateUserListRequestDto { ListTitle = " " }))
            .Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AddGameAsync_WhenMissingFetchesAndAddsGame()
    {
        var lists = new Mock<IUserListRepository>();
        var games = new Mock<IGameRepository>();
        var catalog = new Mock<IGameCatalogService>();
        var unitOfWork = TestDoubles.UnitOfWork(games: games, userLists: lists);
        lists.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new UserList { Id = 4, UserId = 2 });
        lists.Setup(x => x.ContainsGameByCatalogIdAsync(4, 99)).ReturnsAsync(false);
        lists.Setup(x => x.GetGameCountByListIdAsync(4)).ReturnsAsync(0);
        games.Setup(x => x.GetByCatalogIdAsync(99)).ReturnsAsync((Game?)null);
        catalog.Setup(x => x.GetGameDetailsByCatalogIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GameDBWebApi.DTOs.Game.GameDetailsResponseDto { CatalogId = 99, Title = "Game" });
        var service = new UserListService(unitOfWork.Object, catalog.Object);

        await service.AddGameAsync(2, 4, 99, CancellationToken.None);

        games.Verify(x => x.AddAsync(It.Is<Game>(g => g.CatalogId == 99)), Times.Once);
        lists.Verify(x => x.AddGameAsync(4, It.IsAny<int>()), Times.Once);
        unitOfWork.Verify(x => x.CompleteAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteListAsync_WhenNotOwnedThrowsForbidden()
    {
        var lists = new Mock<IUserListRepository>();
        lists.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(new UserList { Id = 4, UserId = 8 });
        var service = new UserListService(TestDoubles.UnitOfWork(userLists: lists).Object, Mock.Of<IGameCatalogService>());

        await service.Invoking(x => x.DeleteListAsync(2, 4)).Should().ThrowAsync<ForbiddenException>();
    }
}
