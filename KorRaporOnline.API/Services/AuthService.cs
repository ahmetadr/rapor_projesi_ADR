using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using KorRaporOnline.API.Data;
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Models.DTOs;
using KorRaporOnline.API.Services.Interfaces;
using System.Collections.Generic;
 

namespace KorRaporOnline.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResponse<AuthResponse>> RegisterUserAsync(UserRegistrationDto model)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    return new ServiceResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password),
                    Email = model.Email,
                    FullName = model.FullName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                var roles = await _context.UserRoles
                    .Where(ur => ur.UserID == user.UserID)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();

                return new ServiceResponse<AuthResponse>
                {
                    Data = new AuthResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        UserId = user.UserID,
                        Username = user.Username,
                        Email = user.Email,
                        Roles = roles
                    },
                    Success = true,
                    Message = "Registration successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User registration failed");
                return new ServiceResponse<AuthResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<AuthResponse>> LoginAsync(UserLoginDto model)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user == null || !VerifyPasswordHash(model.Password, user.PasswordHash))
                {
                    return new ServiceResponse<AuthResponse>
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                var roles = await _context.UserRoles
                    .Where(ur => ur.UserID == user.UserID)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();

                return new ServiceResponse<AuthResponse>
                {
                    Data = new AuthResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        UserId = user.UserID,
                        Username = user.Username,
                        Email = user.Email,
                        Roles = roles
                    },
                    Success = true,
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return new ServiceResponse<AuthResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<User> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            return user;
        }
        public async Task<ServiceResponse<TokenResponse>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return new ServiceResponse<TokenResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return new ServiceResponse<TokenResponse>
                {
                    Data = new TokenResponse
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken
                    },
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TokenResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!VerifyPasswordHash(currentPassword, user.PasswordHash))
            {
                throw new Exception("Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email)
    };

            // Add role claims
            var userRoles = _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserID == user.UserID)
                .Select(ur => ur.Role.Name);

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
                SigningCredentials = creds,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[64];
            Array.Copy(hashBytes, 0, salt, 0, salt.Length);

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hashBytes[salt.Length + i])
                    return false;
            }

            return true;
        }


       

    }
}