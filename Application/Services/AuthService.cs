using Application.DTOs;
using Config;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;


        private readonly JwtUtils _jwtUtils;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public AuthService(IUserRepository userRepository, JwtUtils jwtUtils, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IRefreshTokenRepository refreshTokenRepository, JwtSettings jwtSettings, ILogger<AuthService> logger)
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

            var user = await _userManager.FindByNameAsync(loginRequest.UserName) ?? throw new EntityNotFoundException("User not found");

            // Nếu user không tồn tại, vẫn check fake password để tránh timing attack
            if (user == null)
            {
                _logger.LogWarning("Invalid login attempt - user not found: {UserName}", loginRequest.UserName);
                await Task.Delay(200); // nhẹ để tránh timing attack
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

            var userRoles = await _userManager.GetRolesAsync(user);

            string refreshToken = _jwtUtils.GenerateRefreshToken();

            await SaveRefreshTokenToDatabase(refreshToken, user);

            _logger.LogInformation("User {UserName} logged in successfully.", loginRequest.UserName);

            return new AuthResponse
            {
                IsSuccess = true,
                Message = "Login Successfully",
                AccessToken = _jwtUtils.GenerateAccessToken(user.Id, user.UserName!, userRoles),
                RefreshToken = refreshToken
            };

        }

        private async Task SaveRefreshTokenToDatabase(string refreshToken, ApplicationUser user)
        {
            await _refreshTokenRepository.Add(new RefreshToken
            {
                RefreshTokenString = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
                UserId = user.Id,
                User = user
            });
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDTO registerRequest)
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

            var userRoles = await _userManager.GetRolesAsync(newUser);

            string refreshToken = _jwtUtils.GenerateRefreshToken();
            await SaveRefreshTokenToDatabase(refreshToken, newUser);

            return new AuthResponse
            {
                IsSuccess = true,
                Message = "Registration Successfully",
                AccessToken = _jwtUtils.GenerateAccessToken(newUser.Id, newUser.UserName!, userRoles),
                RefreshToken = _jwtUtils.GenerateRefreshToken()
            };
        }
    }
}
