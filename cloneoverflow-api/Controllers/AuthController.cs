using Application.DTOs;
using Application.Services;
using Azure.Core;
using Config;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace cloneoverflow_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;


        public AuthController(IConfiguration config, AuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        [HttpPost(Name = "login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginRequest)
        {
            AuthResponse res = await _authService.LoginAsync(loginRequest);

            if (res.IsSuccess)
            {
                SetCookie("accessToken", res.AccessToken, int.Parse(_config["Jwt:AccessTokenExpireMinutes"]));
                SetCookie("refreshToken", res.RefreshToken, int.Parse(_config["Jwt:RefreshTokenExpireDays"]) * 24 * 60);

                return Ok();
            }
            return Unauthorized();
        }




        private void SetCookie(string key, string value, int expireMinutes)
        {
            Response.Cookies.Append(key, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Path = "/"
            });
        }
    }
}
