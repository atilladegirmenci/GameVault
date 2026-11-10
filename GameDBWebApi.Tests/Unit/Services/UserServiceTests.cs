using FluentAssertions;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.DTOs.User;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;
using GameDBWebApi.Services;
using Moq;
using Microsoft.Extensions.Configuration;

namespace GameDBWebApi.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task GetUserByIdAsync_MapsProfileAndGames()
    {
        var users = new Mock<IUserRepository>();
        var userGames = new Mock<IUserGameRepository>();
        users.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(new User { Id = 3, UserName = "sam", UserEmail = "sam@test" });
        userGames.Setup(x => x.GetByUserIdAsync(3, 1, 20)).ReturnsAsync(new DTOs.PaginatedList<UserGame>(
            [new UserGame { Game = new Game { Id = 9, CatalogId = 99, Title = "Title" }, Status = GameStatus.Finished }], 1, 1, 20));
        var service = new UserService(TestDoubles.UnitOfWork(users: users, userGames: userGames).Object,
            TestDoubles.Logger<UserService>(), Mock.Of<IConfiguration>());

        var result = await service.GetUserByIdAsync(3);

        result.UserName.Should().Be("sam");
        result.UserGames.Should().ContainSingle(x => x.CatalogId == 99 && x.Status == GameStatus.Finished);
    }

    [Fact]
    public async Task GetUserByIdAsync_MissingUserThrowsNotFound()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(3)).ReturnsAsync((User?)null);
        var service = new UserService(TestDoubles.UnitOfWork(users: users).Object,
            TestDoubles.Logger<UserService>(), Mock.Of<IConfiguration>());

        await service.Invoking(x => x.GetUserByIdAsync(3)).Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmailDoesNotPersist()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(new User { Id = 3, UserEmail = "old@test" });
        users.Setup(x => x.EmailExistsAsync("new@test")).ReturnsAsync(true);
        var unitOfWork = TestDoubles.UnitOfWork(users: users);
        var service = new UserService(unitOfWork.Object, TestDoubles.Logger<UserService>(), Mock.Of<IConfiguration>());

        await service.Invoking(x => x.UpdateAsync(3, new UpdateRequestDto { Email = "new@test", Username = "new" }))
            .Should().ThrowAsync<ConflictException>();
        unitOfWork.Verify(x => x.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUserDeletesAndSaves()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(new User { Id = 3 });
        var unitOfWork = TestDoubles.UnitOfWork(users: users);
        var service = new UserService(unitOfWork.Object, TestDoubles.Logger<UserService>(), Mock.Of<IConfiguration>());

        await service.DeleteAsync(3);

        users.Verify(x => x.DeleteAsync(It.Is<User>(u => u.Id == 3)), Times.Once);
        unitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
    }
}
