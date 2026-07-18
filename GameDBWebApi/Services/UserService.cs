using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Entites;
using GameDBWebApi.Repositories;

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

        //TODO: create a dto to return only the necessary user information instead of returning the entire User entity
        public async Task<User> GetUserByIdAsync(int id)
        {
            var  user = await _unitOfWork.Users.GetByIdAsync(id);
            if(user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
                throw new Exception("User not found.");
            }
            return user;
        }

        public async Task UpdateAsync(int id, UpdateRequestDto request)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser == null)
            {
                _logger.LogWarning("Update operation for user with ID {UserId} failed. User was not found.", id);
                throw new Exception("User not found.");
            }

            if (existingUser.UserEmail != request.Email)
            {
                bool emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Update operation for user with ID {UserId} failed. Email {Email} already exists.", id, request.Email);
                    throw new Exception("Email already exists.");
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
                throw new Exception("User not found.");
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
        }
}
}
