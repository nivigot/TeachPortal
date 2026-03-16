using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TeachPortal.DataStore;
using TeachPortal.Models.Interfaces;
using TeachPortal.Models.Models;

namespace TeachPortal.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext dbContext, IConfiguration config, ILogger<AuthService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<string>> SignupAsync(Teacher teacher)
        {
            try
            {
                if (teacher is null)
                {
                    _logger.LogWarning("Signup failed: teacher payload is null.");
                    return new Result<string>(false, "Invalid teacher data.", statusCode: 400);
                }

                _logger.LogInformation("Registering new teacher: {UserName}, {Email}", teacher.UserName, teacher.Email);

                teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(teacher.PasswordHash);

                await _dbContext.Teachers.AddAsync(teacher);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Teacher registered successfully: {UserName}", teacher.UserName);
                return new Result<string>(true, "Teacher registered successfully.", statusCode: 201);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Duplicate key violation during signup for: {UserName}, {Email}", teacher?.UserName, teacher?.Email);
                return new Result<string>(false, "Email or username is already registered.", statusCode: 409);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during signup for: {UserName}", teacher?.UserName);
                return new Result<string>(false, "An unexpected error occurred. Please try again.", statusCode: 500);
            }
        }

        public async Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request is null)
                {
                    _logger.LogWarning("Login attempt with null request.");
                    return new Result<string>(false, "Invalid login request.", statusCode: 400);
                }

                var username = request.Username.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Login attempt with missing credentials.");
                    return new Result<string>(false, "Username and password are required.", statusCode: 400);
                }

                _logger.LogInformation("Login attempt for '{UserName}'.", username);

                var teacher = await _dbContext.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserName == username, ct);

                if (teacher is null || !BCrypt.Net.BCrypt.Verify(request.Password, teacher.PasswordHash))
                {
                    _logger.LogWarning("Login failed: invalid credentials for '{UserName}'.", username);
                    return new Result<string>(false, "Invalid username or password.", statusCode: 401);
                }

                var token = GenerateJwtToken(teacher);

                _logger.LogInformation("Login successful for '{UserName}'.", username);
                return new Result<string>(true, "Login successful.", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for '{UserName}'.", request?.Username);
                return new Result<string>(false, "An unexpected error occurred. Please try again.", statusCode: 500);
            }
        }

        private string GenerateJwtToken(Teacher teacher)
        {
            var secret = _config["Jwt:Secret"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT secret is not configured.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, teacher.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, teacher.Id.ToString()),
                new Claim(ClaimTypes.Name, teacher.UserName ?? teacher.Email),
                new Claim(ClaimTypes.Email, teacher.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
