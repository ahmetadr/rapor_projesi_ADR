using System.Threading.Tasks;
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Models.DTOs;

namespace KorRaporOnline.API.Services.Interfaces
{
    public interface IAuthService
    {


        Task<ServiceResponse<AuthResponse>> RegisterUserAsync(UserRegistrationDto model);
        Task<ServiceResponse<AuthResponse>> LoginAsync(UserLoginDto model);
        Task<User> GetUserById(int id);
        Task<ServiceResponse<TokenResponse>> RefreshTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
 
    }
}