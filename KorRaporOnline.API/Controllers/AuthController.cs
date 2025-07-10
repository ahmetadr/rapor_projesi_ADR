using KorRaporOnline.API.Models.DTOs;
using KorRaporOnline.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;

namespace KorRaporOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.RegisterUserAsync(registrationDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error during registration", error = ex.Message });
            }
        }
        [HttpPost("login")]
        [EnableRateLimiting("LoginRateLimiter")] // Rate limiting eklenmeli
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.LoginAsync(loginDto);
                if (result.Success)
                {
                    return Ok(result);
                }
                return Unauthorized(new { message = "Invalid login attempt" }); // Daha güvenli hata mesajı
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred during login" }); // Exception detayları gizlendi
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(new { token = result });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while refreshing token" });
            }
        }



    }
}