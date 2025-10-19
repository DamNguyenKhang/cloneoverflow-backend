using Application.DTOs;
using Config;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    public class AccountService : IAccountService
    {

        private readonly IUserRepository _userRepository;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;


        private readonly JwtUtils _jwtUtils;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AccountService> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public AccountService(IUserRepository userRepository, JwtUtils jwtUtils, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserRefreshTokenRepository refreshTokenRepository, JwtSettings jwtSettings, ILogger<AccountService> logger)
        {
            _userRepository = userRepository;
            _jwtUtils = jwtUtils;
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtSettings = jwtSettings;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginDTO loginRequest)
        {
            // Log user attempt
            _logger.LogInformation("Login attempt for username: {UserName}", loginRequest.UserName);

            var user = await _userManager.FindByNameAsync(loginRequest.UserName);

            // Nếu user không tồn tại, vẫn check fake password để tránh timing attack
            if (user == null)
            {
                _logger.LogWarning("Invalid login attempt - user not found: {UserName}", loginRequest.UserName);

                // fake check passwork hash
                //            _userManager.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), new PasswordHasher<ApplicationUser>()
                //.HashPassword(new ApplicationUser(), "fake_password"), "fake");

                await Task.Delay(200);

                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid username or password."
                };
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, isPersistent: false, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                _logger.LogWarning("Invalid password attempt for user: {UserName}", loginRequest.UserName);
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid username or password."
                };
            }

            return await CreateSuccessfulAuthResponse(user);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDTO registerRequest)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var existingUser = await _userManager.FindByNameAsync(registerRequest.UserName);
                if (existingUser != null)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = "User already exists!"
                    };
                }

                var newUser = new ApplicationUser
                {
                    UserName = registerRequest.UserName,
                    FullName = registerRequest.FullName,
                    Email = registerRequest.Email
                };

                var createResult = await _userManager.CreateAsync(newUser, registerRequest.Password);

                if (!createResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = $"Registration failed: {errorMessage}"
                    };
                }

                await _userManager.AddToRoleAsync(newUser, "User");

                // sign in sau khi dang ky
                await _signInManager.SignInAsync(newUser, isPersistent: false);

                var response = await CreateSuccessfulAuthResponse(newUser);

                scope.Complete();

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed with exception for username: {UserName}", registerRequest.UserName);
                throw;
            }
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshTokenStr)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(refreshTokenStr);

            UserRefreshToken? userRefreshToken = await _refreshTokenRepository.GetByTokenStringAsync(refreshTokenStr);

            if (userRefreshToken == null)
            {
                UserRefreshToken? oldRefreshToken = await _refreshTokenRepository.GetByOldTokenStringAsync(refreshTokenStr);

                if (oldRefreshToken != null)
                {
                    _logger.LogWarning("Refresh token is old => CO NGUOI VAO NICK NE {UserName}", oldRefreshToken.User.UserName);
                }
                throw new EntityNotFoundException("Refresh token is not found");
            }

            if (userRefreshToken.IsExpired())
            {
                throw new SecurityTokenException("Refresh token has expired");
            }

            var roles = await _userManager.GetRolesAsync(userRefreshToken.User);

            var newAccessToken = _jwtUtils.GenerateAccessToken(userRefreshToken.User.Id, userRefreshToken.User.UserName, roles);

            userRefreshToken = await UpdateUserRefreshTokenAsync(userRefreshToken);

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                UserRefreshToken = userRefreshToken
            };
        }

        public async Task<bool> LogOutAsync(string refreshTokenStr)
        {

            ArgumentNullException.ThrowIfNullOrEmpty(refreshTokenStr);

            UserRefreshToken? userRefreshToken = await _refreshTokenRepository.GetByTokenStringAsync(refreshTokenStr);

            if (userRefreshToken == null)
            {
                throw new EntityNotFoundException("Refresh token is not found");
            }

            await _refreshTokenRepository.Delete(userRefreshToken);

            return await Task.FromResult(true);
        }

        private async Task<AuthResponse> CreateSuccessfulAuthResponse(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var userRefreshToken = await AddUserRefreshTokenToDatabaseAsync(user);

            return new AuthResponse
            {
                IsSuccess = true,
                Message = "Login Successfully",
                AccessToken = _jwtUtils.GenerateAccessToken(user.Id, user.UserName!, userRoles),
                UserRefreshToken = userRefreshToken
            };
        }

        private async Task<UserRefreshToken> UpdateUserRefreshTokenAsync(UserRefreshToken userRefreshToken)
        {
            userRefreshToken.ReplacedByToken = userRefreshToken.RefreshTokenString;
            userRefreshToken.RefreshTokenString = _jwtUtils.GenerateRefreshToken();
            userRefreshToken.CreatedAt = DateTime.UtcNow;
            userRefreshToken.ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays);

            return await _refreshTokenRepository.Update(userRefreshToken);
        }

        private async Task<UserRefreshToken> AddUserRefreshTokenToDatabaseAsync(ApplicationUser user)
        {
            string refreshToken = _jwtUtils.GenerateRefreshToken();
            return await _refreshTokenRepository.Add(new UserRefreshToken
            {
                RefreshTokenString = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
                UserId = user.Id,
                User = user
            });
        }

    }
}