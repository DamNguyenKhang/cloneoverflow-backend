using System.Runtime.CompilerServices;
using Application;
using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Services.Interfaces;
using Azure.Core;
using Config;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;



namespace cloneoverflow_api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ICookieService _cookieService;

        private readonly JwtSettings _jwtSettings;


        public AccountController(IAccountService accountService, ICookieService cookieService, JwtSettings jwtSettings)
        {
            _accountService = accountService;
            _cookieService = cookieService;

            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<bool>>> Login([FromBody] LoginRequest loginRequest)
        {
            AuthResponse res = await _accountService.LoginAsync(loginRequest);
            _cookieService.SetCookie(Response, "accessToken", res.AccessToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes));
            _cookieService.SetCookie(Response, "refreshToken", res.UserRefreshToken.RefreshTokenString, res.UserRefreshToken.ExpiresAt);

            return Ok(new ApiResponse<bool>
            {
                Result = true,
                Message = "Login successfully"
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<bool>>> Register([FromBody] RegisterRequest registerRequest)
        {
            AuthResponse res = await _accountService.RegisterAsync(registerRequest);

            _cookieService.SetCookie(Response, "accessToken", res.AccessToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes));
            _cookieService.SetCookie(Response, "refreshToken", res.UserRefreshToken.RefreshTokenString, res.UserRefreshToken.ExpiresAt);

            return Ok(new ApiResponse<bool>
            {
                Result = true,
                Message = "Register successfully"
            });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<bool>>> RefreshToken()
        {
            var refreshTokenStr = Request.Cookies["refreshToken"];

            TokenResponse res = await _accountService.RefreshTokenAsync(refreshTokenStr);
            _cookieService.SetCookie(Response, "accessToken", res.AccessToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes));
            _cookieService.SetCookie(Response, "refreshToken", res.UserRefreshToken.RefreshTokenString, res.UserRefreshToken.ExpiresAt);

            return Ok(new ApiResponse<bool>
            {
                Result = true,
                Message = "Refresh token successfully"
            });

        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> LogOut()
        {
            var refreshTokenStr = Request.Cookies["refreshToken"];
            bool res = await _accountService.LogOutAsync(refreshTokenStr);

            _cookieService.SetCookie(Response, "accessToken", "", DateTime.UtcNow.AddMinutes(-1));
            _cookieService.SetCookie(Response, "refreshToken", "", DateTime.UtcNow.AddMinutes(-1));

            return Ok(new ApiResponse<bool>
            {
                Result = true,
                Message = "Logout successfully"
            });
        }
    }
}
