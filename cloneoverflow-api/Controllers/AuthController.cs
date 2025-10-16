using Application.DTOs;
using Application.Services;
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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICookieService _cookieService;

        private readonly JwtSettings _jwtSettings;


        public AuthController(IAuthService authService, ICookieService cookieService, JwtSettings jwtSettings)
        {
            _authService = authService;
            _cookieService = cookieService;

            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginRequest)
        {
            AuthResponse res = await _authService.LoginAsync(loginRequest);

            if (res.IsSuccess)
            {
                _cookieService.SetCookie(Response, "accessToken", res.AccessToken, _jwtSettings.AccessTokenExpireMinutes);
                _cookieService.SetCookie(Response, "refreshToken", res.RefreshToken, _jwtSettings.RefreshTokenExpireDays * 24 * 60);

                return Ok(new { Success = true, Message = res.Message });
            }
            return Unauthorized(new { Success = false, Message = res.Message });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerRequest)
        {
            AuthResponse res = await _authService.RegisterAsync(registerRequest);

            if (res.IsSuccess)
            {
                _cookieService.SetCookie(Response, "accessToken", res.AccessToken, _jwtSettings.AccessTokenExpireMinutes);
                _cookieService.SetCookie(Response, "refreshToken", res.RefreshToken, _jwtSettings.RefreshTokenExpireDays * 24 * 60);

                return Ok(new { Success = true, Message = res.Message });
            }
            return Unauthorized(new { Success = false, Message = res.Message });
        }
    }
}
