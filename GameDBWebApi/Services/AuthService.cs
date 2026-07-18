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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }
        //TODO: user email to login instead of username, and add email validation during registration. maybe email and username both can be used to login, but email should be unique and username should be unique as well.
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Attempting login for user: {Username}", request.Username);
            var user = await _unitOfWork.Users.GetByUserNameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword))
            {
                _logger.LogWarning("Invalid credentials for user: {Username}", request.Username);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            string token = GenerateJwtToken(user);
            _logger.LogInformation("User {Username} logged in successfully. Token generated.", request.Username);
            return new LoginResponseDto
            {
                Token = token,
                Message = "Login successful."
            };
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Attempting to register user: {Username}, {Email}", request.Username, request.Email);
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                throw new Exception("Email already exists.");
            }

            
            var newUser = new User
            {
                UserName = request.Username,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserEmail = request.Email

            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync(); 

            _logger.LogInformation("User {Username} registered successfully.", request.Username);

        }
      
        public async Task LogoutAsync()
        {
            // In a stateless JWT authentication system, logout is typically handled on the client side by deleting the token.
            // However, if you want to implement server-side logout, you can maintain a blacklist of tokens or use a token revocation strategy.
            // For this example, we'll just simulate a logout operation.
           
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
