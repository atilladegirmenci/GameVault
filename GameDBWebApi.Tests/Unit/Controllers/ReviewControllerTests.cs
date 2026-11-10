using FluentAssertions;
using GameDBWebApi.Controllers;
using GameDBWebApi.DTOs.Review;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace GameDBWebApi.Tests.Unit.Controllers;

public class ReviewControllerTests
{
    [Fact]
    public async Task InvalidCatalogIdReturnsBadRequest()
    {
        var result = await new ReviewController(Mock.Of<IReviewService>())
            .GetGameReviews(0, 1, 10, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateReviewUsesClaimedUser()
    {
        var reviews = new Mock<IReviewService>();
        reviews.Setup(x => x.CreateReviewAsync(4, 10, It.IsAny<CreateReviewRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReviewResponseDto { Id = 9 });
        var controller = new ReviewController(reviews.Object) { ControllerContext = Context(4) };

        var result = await controller.CreateReview(10, new CreateReviewRequestDto(), CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        reviews.VerifyAll();
    }

    private static ControllerContext Context(int id) =>
        new() { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test")) } };
}
