using GameDBWebApi.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameDBWebApi.Tests;

internal static class TestDoubles
{
    public static Mock<IUnitOfWork> UnitOfWork(
        Mock<IUserRepository>? users = null,
        Mock<IGameRepository>? games = null,
        Mock<IUserGameRepository>? userGames = null,
        Mock<IReviewRepository>? reviews = null,
        Mock<IUserListRepository>? userLists = null,
        Mock<IAccountTokenRepository>? tokens = null)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Users).Returns((users ?? new Mock<IUserRepository>()).Object);
        unitOfWork.SetupGet(x => x.Games).Returns((games ?? new Mock<IGameRepository>()).Object);
        unitOfWork.SetupGet(x => x.UserGames).Returns((userGames ?? new Mock<IUserGameRepository>()).Object);
        unitOfWork.SetupGet(x => x.Reviews).Returns((reviews ?? new Mock<IReviewRepository>()).Object);
        unitOfWork.SetupGet(x => x.UserLists).Returns((userLists ?? new Mock<IUserListRepository>()).Object);
        unitOfWork.SetupGet(x => x.AccountTokens)
            .Returns((tokens ?? new Mock<IAccountTokenRepository>()).Object);
        return unitOfWork;
    }

    public static ILogger<T> Logger<T>() => new Mock<ILogger<T>>().Object;
}
