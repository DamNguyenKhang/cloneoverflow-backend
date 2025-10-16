using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Config
{
    public class JwtUtils
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtUtils> _logger;

        public JwtUtils(JwtSettings jwtSettings, ILogger<JwtUtils> logger)
        {
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger;
        }

        public String GenerateAccessToken(String userId, String userName, IList<string> roles)
        {

            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Username cannot be null or empty.", nameof(userName));
            if (roles == null || !roles.Any()) throw new ArgumentException("Roles cannot be null or empty.", nameof(roles));

            _logger.LogInformation("JWT Key length: {Length}", _jwtSettings.Key?.Length);
            if (_jwtSettings.Key.Length < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 characters long.");

            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName),
                new Claim(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
