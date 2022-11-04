
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Compass.Data.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<IdentityResult> RegisterUserAsync(AppUser model, string password);
        Task<AppUser> LoginUserAsync(LoginUserVM model);
        Task<bool> ValidatePasswordAsync(LoginUserVM model, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(AppUser appUser);
        Task<AppUser> GetUserByIdAsync(string id);
        Task<IList<string>> GetRolesAsync(AppUser model);
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<IdentityResult> ConfirmEmailAsync(AppUser model, string token);
        Task<string> GeneratePasswordResetTokenAsync(AppUser model);
        Task<IdentityResult> ResetPasswordAsync(AppUser model, string token, string password);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken> CheckRefreshTokenAsync(string refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task<List<AppUser>> GetAllUsersAsync(int start, int end, bool isAll = false);
        Task<bool> SetUserRoleAsync(AppUser model, string role);
        Task<AppUser> GetUserProfileAsync(string userId);
    }
}
