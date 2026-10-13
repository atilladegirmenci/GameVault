using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using GameDBWebApi.Services;
using GameDBWebApi.Repositories;
using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Entites;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace GameDBWebApi.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public AuthServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _mockUnitOfWork.Setup(uow => uow.Users).Returns(_mockUserRepository.Object);

            var inMemorySettings = new Dictionary<string, string>
            {
                {"JwtSettings:SecretKey", "SuperSecretTestKeyThatIsAtLeast32CharsLong123!"},
                {"JwtSettings:Issuer", "TestApi"},
                {"JwtSettings:Audience", "TestUsers"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(_mockUnitOfWork.Object, _configuration, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_CreatesUserAndSaves()
        {
            // Arrange
            var registerRequest = new RegisterRequestDto
            {
                Email = "newuser@example.com",
                Password = "TestPassword123!"
            };

            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerRequest.Email)).ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync(registerRequest);

            // Assert
            await act.Should().NotThrowAsync();
            _mockUserRepository.Verify(repo => repo.AddAsync(It.Is<User>(u => u.UserEmail == registerRequest.Email)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsException()
        {
            // Arrange
            var registerRequest = new RegisterRequestDto
            {
                Email = "testuser@example.com",
                Password = "TestPassword123!"
            };

            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerRequest.Email)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync(registerRequest);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Email already exists.");
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                UserMail = "user@example.com",
                Password = "TestPassword123!"
            };
            var user = new User
            {
                Id = 1,
                UserEmail = loginRequest.UserMail,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password)
            };
            _mockUserRepository.Setup(repo => repo.GetByUserMailAsync(loginRequest.UserMail)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.Message.Should().Be("Login successful.");
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                UserMail = "user@example.com",
                Password = "WrongPassword!"
            };
            var user = new User
            {
                Id = 1,
                UserEmail = loginRequest.UserMail,
                HashPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!")
            };

            _mockUserRepository.Setup(repo => repo.GetByUserMailAsync(loginRequest.UserMail)).ReturnsAsync(user);

            // Act & Assert
            var act = async () => await _authService.LoginAsync(loginRequest);
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                UserMail = "wrong@example.com",
                Password = "Password123!"
            };

            _mockUserRepository.Setup(repo => repo.GetByUserMailAsync(loginRequest.UserMail)).ReturnsAsync((User?)null);

            // Act
            var act = async () => await _authService.LoginAsync(loginRequest);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid email or password.");
        }

        //[Fact]
        //public async Task DeleteAsync_ExistingUser_DeletesUser()
        //{
        //    // Arrange
        //    var userId = 1;
        //    var user = new User
        //    {
        //        Id = userId,
        //        UserEmail = "user@example.com",
        //        HashPassword = BCrypt.Net.BCrypt.HashPassword("TestPassword123!")
        //    };

        //    _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        //    var act = async () => await _authService.DeleteAsync(userId);
        //    await act.Should().NotThrowAsync();
        //    _mockUserRepository.Verify(repo => repo.DeleteAsync(It.IsAny<User>()), Times.Once);
        //    _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        //}

        //[Fact]
        //public async Task DeleteAsync_NonExistingUser_ThrowsException()
        //{
        //    // Arrange
        //    var userId = 999; // Non-existing user ID
        //    _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        //    // Act
        //    var act = async () => await _authService.DeleteAsync(userId);

        //    // Assert
        //    await act.Should().ThrowAsync<Exception>().WithMessage("User not found.");
        //    _mockUserRepository.Verify(repo => repo.DeleteAsync(It.IsAny<User>()), Times.Never);
        //    _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        //}

    //    [Fact]
    //    public async Task UpdateAsync_ValidCredentials_UpdatesUserAndSaves()
    //    {
    //        var userId = 1;
    //        var existingUser = new User
    //        {
    //            Id = userId,
    //            UserEmail = "olduser@example.com",
    //            HashPassword = BCrypt.Net.BCrypt.HashPassword("TestPassword123!")
    //        };
    //        var updateRequest = new UpdateRequestDto
    //        {
    //            Email = "newuser@example.com"
    //        };

    //        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
    //        var act = async () => await _authService.UpdateAsync(userId, updateRequest);
    //        await act.Should().NotThrowAsync();
    //        _mockUserRepository.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.UserEmail == updateRequest.Email)), Times.Once);
    //        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
    //    }

    //    [Fact]
    //    public async Task UpdateAsync_ExistingEmail_ThrowsException()
    //    {
    //        var userId = 1;
    //        var existingUser = new User
    //        {
    //            Id = userId,
    //            UserEmail = "user@example.com",
    //            HashPassword = BCrypt.Net.BCrypt.HashPassword("TestPassword123!")
    //        };

    //        var updateRequest = new UpdateRequestDto
    //        {
    //            Email = "existingmail@example.com"
    //        };

    //        _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
    //        _mockUserRepository.Setup(repo => repo.EmailExistsAsync(updateRequest.Email)).ReturnsAsync(true);
    //        var act = async () => await _authService.UpdateAsync(userId, updateRequest);
    //        await act.Should().ThrowAsync<Exception>().WithMessage("Email already exists.");
    //        _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
    //    }
    }
}