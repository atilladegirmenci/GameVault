using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace GameDBWebApi.Services
{
    public class AuthService : IAuthService
    {
        //private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            if(await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                throw new Exception("Email already exists.");
            }

            
            var newUser = new Entites.User
            {
                UserName = request.Username,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserEmail = request.Email

            };
            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync(); 
            
        }
    }
}
