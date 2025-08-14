using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TeachPortal.DataStore;
using TechPortal.Models.Interfaces;
using TechPortal.Models.Models;

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
                if (teacher == null)
                {
                    _logger.LogError("Invalid teacher data: teacher is null");
                    return new Result<string>(false, "Invalid teacher data");
                }

                _logger.LogInformation("Registering new teacher: {UserName}, {Email}", teacher.UserName, teacher.Email);

                teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(teacher.PasswordHash);

                await _dbContext.Teachers.AddAsync(teacher);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Teacher registered successfully: {UserName}, {Email}", teacher.UserName, teacher.Email);

                return new Result<string>(true, "Teacher registered successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error occurred while registering teacher: {UserName}, {Email}", teacher?.UserName, teacher?.Email);
                return new Result<string>(false, "Database update error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering teacher: {UserName}, {Email}", teacher?.UserName, teacher?.Email);
                return new Result<string>(false, "An error occurred while registering the teacher");
            }
        }

        public async Task<Result<string>> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            try
            {
                if (request is null)
                {
                    _logger.LogWarning("Login failed: request is null.");
                    return new Result<string>(false, "Invalid login request");
                }

                var username = request.Username?.Trim();
                var password = request.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Login failed: missing username or password.");
                    return new Result<string>(false, "Username and password are required");
                }

                _logger.LogInformation("Login attempt for user '{UserName}'.", username);

                var teacher = await _dbContext.Teachers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserName == username, ct);

                if (teacher is null || !BCrypt.Net.BCrypt.Verify(password, teacher.PasswordHash))
                {
                    _logger.LogWarning("Login failed: invalid credentials for '{UserName}'.", username);
                    return new Result<string>(false, "Invalid username or password");
                }

                var token = GenerateJwtToken(teacher);

                _logger.LogInformation("Login success for user '{UserName}'.", username);
                return new Result<string>(true, "Teacher logged in successfully", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for '{UserName}'.", request?.Username);
                return new Result<string>(false, "An error occurred while logging in the teacher");
            }
        }

        private string GenerateJwtToken(Teacher teacher)
        {
            var secret = _config["Jwt:Secret"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 30;

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT secret is not configured.");

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, teacher.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Sub, teacher.Id.ToString()),
        new Claim(ClaimTypes.Name, teacher.UserName ?? teacher.Email ?? "teacher"),
        new Claim(ClaimTypes.Email, teacher.Email ?? string.Empty),
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
