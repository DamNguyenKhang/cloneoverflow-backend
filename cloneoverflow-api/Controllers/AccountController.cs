using Application.DTOs.Requests;
using Application.Services.Interfaces;
using Azure.Core;
using Config;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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
        public async Task<ActionResult> Login([FromBody] Application.DTOs.Requests.LoginRequest loginRequest)
        {
            AuthResponse res = await _accountService.LoginAsync(loginRequest);

            if (res.IsSuccess)
            {
                _cookieService.SetCookie(Response, "accessToken", res.AccessToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes));
                _cookieService.SetCookie(Response, "refreshToken", res.UserRefreshToken.RefreshTokenString, res.UserRefreshToken.ExpiresAt);

                return Ok(new { Success = true, Message = res.Message });
            }
            return Unauthorized(new { Success = false, Message = res.Message });
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] Application.DTOs.Requests.RegisterRequest registerRequest)
        {
            AuthResponse res = await _accountService.RegisterAsync(registerRequest);

            if (res.IsSuccess)
            {
                _cookieService.SetCookie(Response, "accessToken", res.AccessToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes));
                _cookieService.SetCookie(Response, "refreshToken", res.UserRefreshToken.RefreshTokenString, res.UserRefreshToken.ExpiresAt);

                return Ok(new { Success = true, Message = res.Message });
            }
            return Unauthorized(new { Success = false, Message = res.Message });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            var refreshTokenStr = Request.Cookies["refreshToken"];

            TokenResponse res = await _accountService.RefreshTokenAsync(refreshTokenStr);
            _cookieService.SetCookie(Response, "accessToken", res.AccessToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes));
            _cookieService.SetCookie(Response, "refreshToken", res.UserRefreshToken.RefreshTokenString, res.UserRefreshToken.ExpiresAt);

            return Ok();

        }

        [HttpPost("logout")]
        public async Task<ActionResult> LogOut()
        {
            var refreshTokenStr = Request.Cookies["refreshToken"];
            bool res = await _accountService.LogOutAsync(refreshTokenStr);

            _cookieService.SetCookie(Response, "accessToken", "", DateTime.UtcNow.AddMinutes(-1));
            _cookieService.SetCookie(Response, "refreshToken", "", DateTime.UtcNow.AddMinutes(-1));

            return Ok(new { Success = true, Message = "Logout successfully" });
        }
    }
}
