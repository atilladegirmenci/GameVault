using BCrypt.Net;
using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Entites;
using GameDBWebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GameDBWebApi.Services
{
    public class AuthService : IAuthService
    {
        //private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword);
            if(!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            string token = GenerateJwtToken(user);
            return new LoginResponseDto
            {
                Token = token,
                Message = "Login successful."
            };
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
        public async Task DeleteAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CompleteAsync();
        }
        public async Task UpdateAsync(int id, UpdateRequestDto request)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if(existingUser == null)
            {
                throw new Exception("User not found.");
            }

            if(existingUser.UserEmail != request.Email)
            {
                bool emailExists = await _unitOfWork.Users.EmailExistsAsync(request.Email);
                if(emailExists)
                {
                    throw new Exception("Email already exists.");
                }
            }

            existingUser.UserName = request.Username;
            existingUser.UserEmail = request.Email;

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.CompleteAsync();
        }

        private string GenerateJwtToken(User user)
        {
            // Read the secret key from appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "MySuperSecretKeyForGameVaultApi123456789!";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create Claims (The hidden data inside the token)
            // ClaimTypes.NameIdentifier is the standard for storing the User ID
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Configure the Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(6), // Token is valid for 6 hours
                SigningCredentials = credentials,
                Issuer = jwtSettings["Issuer"] ?? "GameVaultApi",
                Audience = jwtSettings["Audience"] ?? "GameVaultUsers"
            };

            // Generate and return the serialized token string
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
