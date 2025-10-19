using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IAccountService
    {
        public Task<AuthResponse> LoginAsync(LoginDTO loginRequest);
        public Task<AuthResponse> RegisterAsync(RegisterDTO registerRequest);
        public Task<TokenResponse> RefreshTokenAsync(string refreshTokenStr);
        public Task<bool> LogOutAsync(string refreshTokenStr);
    }
}
