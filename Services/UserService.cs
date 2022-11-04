using AutoMapper;
using Compass.Data.Data.Interfaces;
using Compass.Data.Data.Models;
using Compass.Data.Data.ViewModels;
using Compass.Services.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace Compass.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private IConfiguration _configuration;
        private EmailService _emailService;
        private JwtService _jwtService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, JwtService jwtService, IConfiguration configuration, EmailService emailService, IMapper mapper, IOptionsMonitor<JwtConfig> optionsMonitor, TokenValidationParameters tokenValidationParameters)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
            _jwtService = jwtService;
            _mapper = mapper;
        }
        public async Task<ServiceResponse> RegisterUserAsync(RegisterUserVM model)
        {
            if (model == null)
            {
                throw new NullReferenceException("Register model is null.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new ServiceResponse
                {
                    Message = "Confirm pssword do not match",
                    IsSuccess = false
                };
            }

            var newUser = _mapper.Map<RegisterUserVM, AppUser>(model);

            var result = await  _userRepository.RegisterUserAsync(newUser, model.Password);
            
            if (result.Succeeded)
            {
                 var addRoleResult = await _userRepository.SetUserRoleAsync(newUser, model.Role);
                if(!addRoleResult)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Set user role error"
                    };
                }
                var token = await _userRepository.GenerateEmailConfirmationTokenAsync(newUser);

                var encodedEmailToken = Encoding.UTF8.GetBytes(token);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                string url = $"{_configuration["HostSettings:URL"]}/api/User/confirmemail?userid={newUser.Id}&token={validEmailToken}";

                string emailBody = $"<h1>Confirm your email</h1> <a href='{url}'>Confirm now</a>";
                await _emailService.SendEmailAsync(newUser.Email, "Email confirmation.", emailBody);

                var tokens = await _jwtService.GenerateJwtTokenAsync(newUser);

                return new ServiceResponse
                {
                    AccessToken = tokens.token,
                    RefreshToken = tokens.refreshToken.Token,
                    Message = "User successfully created.",
                    IsSuccess = true
                };
            }
            else
            {
                return new ServiceResponse
                {
                    Message = "Error user not created.",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }

        public async Task<ServiceResponse> LoginUserAsync(LoginUserVM model)
        {
            var user = await _userRepository.LoginUserAsync(model);

            if (user == null)
            {
                return new ServiceResponse
                {
                    Message = "Login incorrect.",
                    IsSuccess = false
                };
            }

            var result = await _userRepository.ValidatePasswordAsync(model, model.Password);
            if (!result)
            {
                return new ServiceResponse
                {
                    Message = "Password incorrect.",
                    IsSuccess = false
                };
            }

            var tokens = await _jwtService.GenerateJwtTokenAsync(user);

            return new ServiceResponse
            {
                AccessToken = tokens.token,
                RefreshToken = tokens.refreshToken.Token,
                Message = "Logged in successfully",
                IsSuccess = true,
            };
        }

        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "User not found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userRepository.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new ServiceResponse
                {
                    Message = "Email confirmed successfully!",
                    IsSuccess = true,
                };

            return new ServiceResponse
            {
                IsSuccess = false,
                Message = "Email did not confirm",
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<ServiceResponse> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if(user == null)
            {
                return new ServiceResponse
                {
                    Message = "No user associated with email",
                    IsSuccess = false
                };
            }

            var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_configuration["HostSettings:URL"]}/ResetPassword?email={email}&token={validToken}";
            string emailBody = "<h1>Follow the instructions to reset your password</h1>" + $"<p>To reset your password <a href='{url}'>Click here</a></p>";
            await _emailService.SendEmailAsync(email, "Fogot password", emailBody);

            return new ServiceResponse
            {
                IsSuccess = true,
                Message = $"Reset password for {_configuration["HostSettings:URL"]} has been sent to the email successfully!"
            };
        }

        public async Task<ServiceResponse> ResetPasswordAsync(ResetPasswordVM model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if(user == null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Password doesn't match its confirmation",
                };
            }

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userRepository.ResetPasswordAsync(user, normalToken, model.NewPassword);
            if (result.Succeeded)
            {
                return new ServiceResponse
                {
                    Message = "Password has been reset successfully!",
                    IsSuccess = true,
                };
            }
            return new ServiceResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }

        public async Task<ServiceResponse> RefreshTokenAsync(TokenRequestVM model)
        {
            var result = await _jwtService.VerifyTokenAsync(model);
            if(result == null)
            {
                return result;
            }
            else
            {
                return result;
            }
        }

        public async Task<ServiceResponse> GetAllUsersAsync(int start, int end, bool isAll = false)
        {
            List<AppUser> users = await _userRepository.GetAllUsersAsync(start, end, isAll);
            List<GetAllUsersVM> mappedUsers = users.Select(u => _mapper.Map<AppUser, GetAllUsersVM>(u)).ToList();

            for(int i = 0; i < users.Count; i++)
            {
                mappedUsers[i].Role = (await _userRepository.GetRolesAsync(users[i])).FirstOrDefault();
            }

            return new ServiceResponse()
            {
                IsSuccess = true,
                Message = mappedUsers
            };
        }

        public async Task<ServiceResponse> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userRepository.GetUserProfileAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse()
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                var newUser = _mapper.Map<AppUser, UserProfileVM>(user);
                return new ServiceResponse()
                {
                    IsSuccess = true,
                    Payload = newUser,
                    Message = "success"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse()
                {
                    IsSuccess = false,
                    Message= ex.Message
                };
            }
        }

    }
}
