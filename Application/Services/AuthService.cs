using Application.DTOs;
using Config;
using Domain;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
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
        private readonly JwtUtils _jwtUtils;

        private readonly UserManager<ApplicationUser> _userManager;


        public AuthService(IUserRepository userRepository, JwtUtils jwtUtils, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _jwtUtils = jwtUtils;
            _userManager = userManager;
        }

        public async Task<AuthResponse> LoginAsync(LoginDTO loginRequest)
        {
            var user = await _userRepository.FindByUserNameAsync(loginRequest.UserName) ?? throw new EntityNotFoundException("User not found");

            var valid = _userRepository.CheckPasswordAsync(user, loginRequest.Password);

            if (!valid)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Please check your password!"
                };
            }

            var userRoles = await GetUserRolesAsync(user.Id);

            return new AuthResponse
            {
                IsSuccess = true,
                Message = "Login Successfully",
                AccessToken = _jwtUtils.GenerateAccessToken(user.Id, user.UserName!, userRoles),
                RefreshToken = _jwtUtils.GenerateRefreshToken()
            };

        }


        private async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var user = await _userRepository.FindByID(userId) ?? throw new EntityNotFoundException("User not found");

            return await _userManager.GetRolesAsync(user);
        }
    }
}
