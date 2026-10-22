using GameDBWebApi.Entites;
using GameDBWebApi.Repositories;
using GameDBWebApi.DTOs.User;
using GameDBWebApi.Exceptions;

namespace GameDBWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
        }

        // Return a DTO with profile info and a short list of the user's games
        public async Task<UserProfileDto> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                throw new NotFoundException("User not found.");
            }

            // fetch user's games (first page, limited size)
            const int page = 1;
            const int pageSize = 20;
            var userGamesPaginated = await _unitOfWork.UserGames.GetByUserIdAsync(id, page, pageSize);

            var gameDtos = userGamesPaginated.Select(ug => new DTOs.Game.GameCardDto
            {
                RawgId = ug.Game.RawgId,
                Title = ug.Game.Title,
                ImageUrl = ug.Game.ImageUrl,
                Score = ug.Game.Score,
                GameId = ug.Game.Id,
                Status = ug.Status,
                UpdatedDate = ug.UpdatedDate
            }).ToList();

            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.UserEmail,
                UserGames = gameDtos
            };
        }

        public async Task UpdateAsync(int id, UpdateRequestDto request)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("Update operation for user with ID {UserId} failed. User was not found.", id);
                throw new NotFoundException("User not found.");
            }

            if (existingUser.UserEmail != request.Email)
            {
                bool emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Update operation for user with ID {UserId} failed. Email {Email} already exists.", id, request.Email);
                    throw new ConflictException("Email already exists.");
                }
            }


            existingUser.UserName = request.Username;
            existingUser.UserEmail = request.Email;

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User with ID {UserId} updated successfully.", id);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("delete operation for user with ID {UserId} failed. User was not found.", id);
                throw new NotFoundException("User not found.");
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
        }
}
}
