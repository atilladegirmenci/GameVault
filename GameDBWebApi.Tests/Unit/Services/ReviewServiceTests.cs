using FluentAssertions;
using GameDBWebApi.DTOs.Review;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;
using GameDBWebApi.Services;
using Moq;

namespace GameDBWebApi.Tests;

public class ReviewServiceTests
{
    [Fact]
    public async Task CreateReviewAsync_CreatesReviewForCatalogGame()
    {
        var games = new Mock<IGameRepository>();
        var reviews = new Mock<IReviewRepository>();
        var catalog = new Mock<IGameCatalogService>();
        var unitOfWork = TestDoubles.UnitOfWork(games: games, reviews: reviews);
        games.Setup(x => x.GetByCatalogIdAsync(10)).ReturnsAsync(new Game { Id = 5, CatalogId = 10 });
        reviews.Setup(x => x.UserReviewExistsAsync(5, 2)).ReturnsAsync(false);
        reviews.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Review { Id = 7, GameId = 5, UserId = 2, Comment = "Great", Rating = 5 });
        var service = new ReviewService(unitOfWork.Object, catalog.Object, TestDoubles.Logger<ReviewService>());

        var result = await service.CreateReviewAsync(2, 10,
            new CreateReviewRequestDto { Rating = 5, Comment = " Great " }, CancellationToken.None);

        result.Id.Should().Be(7);
        reviews.Verify(x => x.AddAsync(It.Is<Review>(r => r.Comment == "Great" && r.GameId == 5)), Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_DuplicateReviewThrowsConflict()
    {
        var games = new Mock<IGameRepository>();
        var reviews = new Mock<IReviewRepository>();
        games.Setup(x => x.GetByCatalogIdAsync(10)).ReturnsAsync(new Game { Id = 5, CatalogId = 10 });
        reviews.Setup(x => x.UserReviewExistsAsync(5, 2)).ReturnsAsync(true);
        var service = new ReviewService(TestDoubles.UnitOfWork(games: games, reviews: reviews).Object,
            Mock.Of<IGameCatalogService>(), TestDoubles.Logger<ReviewService>());

        await service.Invoking(x => x.CreateReviewAsync(2, 10,
            new CreateReviewRequestDto { Rating = 5, Comment = "Great" }, CancellationToken.None))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetGameReviewsAsync_InvalidPageThrowsBadRequest()
    {
        var service = new ReviewService(TestDoubles.UnitOfWork().Object,
            Mock.Of<IGameCatalogService>(), TestDoubles.Logger<ReviewService>());

        await service.Invoking(x => x.GetGameReviewsAsync(10, 0, 10, CancellationToken.None))
            .Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task DeleteReviewAsync_NonOwnerThrowsForbidden()
    {
        var reviews = new Mock<IReviewRepository>();
        reviews.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new Review { Id = 7, UserId = 3 });
        var service = new ReviewService(TestDoubles.UnitOfWork(reviews: reviews).Object,
            Mock.Of<IGameCatalogService>(), TestDoubles.Logger<ReviewService>());

        await service.Invoking(x => x.DeleteReviewAsync(2, 7)).Should().ThrowAsync<ForbiddenException>();
    }
}
