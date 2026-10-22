using Azure.Core;
using BCrypt.Net;
using GameDBWebApi.DTOs.Auth;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Attempting login for user: {Usermail}", request.UserMail);
            var user = await _unitOfWork.Users.GetByUserMailAsync(request.UserMail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword))
            {
                _logger.LogWarning("Invalid credentials for user: {Usermail}", request.UserMail);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            string accessToken = GenerateJwtToken(user);
            string refreshToken = GenerateRefreshToken();

            // Compute the fingerprint
            user.RefreshTokenFingerprint = ComputeTokenFingerprint(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token valid for 7 days

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {UserMail} logged in successfully. Tokens generated.", request.UserMail);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Message = "Login successful."
            };
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // Find user by the fingerprint of the provided refresh token
            var fingerprint = ComputeTokenFingerprint(request.RefreshToken);
            var user = await _unitOfWork.Users.GetByRefreshTokenFingerprintAsync(fingerprint);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            // Generate new tokens (Token Rotation)
            string newAccessToken = GenerateJwtToken(user);
            string newRefreshToken = GenerateRefreshToken();

            // rotate: compute fingerprint and hash, do not store plaintext
            user.RefreshTokenFingerprint = ComputeTokenFingerprint(newRefreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Refresh token rotated successfully for user ID: {UserId}", user.Id);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Message = "Token refreshed successfully."
            };
        }
        public async Task RegisterAsync(RegisterRequestDto request)
        {

            _logger.LogInformation("Attempting to register user: {Username}, {Email}", request.UserName, request.Email);
           
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
            {
                throw new ConflictException("Email already exists.");
            }

            if (await _unitOfWork.Users.UserNameExistsAsync(request.UserName))
            {
                throw new ConflictException("Username is already taken."); // 409 Conflict (veya BadRequestException)
            }

            var newUser = new User
            {
                UserName = request.UserName,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserEmail = request.Email

            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync(); 

            _logger.LogInformation("User {Username} registered successfully.", request.UserName);

        }
      
        public async Task LogoutAsync(int userId)
        {
            // Server-side logout by revoking/clearing the refresh token
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshTokenFingerprint = null;
                user.RefreshTokenExpiryTime = default;
                await _unitOfWork.CompleteAsync();
                
                _logger.LogInformation("User ID {UserId} logged out successfully. Refresh token cleared.", userId);
            }
           
        }

        private string GenerateJwtToken(User user)
        {
            // Read the secret key from appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured."); ;

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
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string ComputeTokenFingerprint(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
