using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Config
{
    public class JwtUtils
    {
        private readonly IConfiguration _config;

        public JwtUtils(IConfiguration config) => _config = config ?? throw new ArgumentNullException(nameof(config));

        public String GenerateAccessToken(String userId, String userName, IList<string> roles)
        {

            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Username cannot be null or empty.", nameof(userName));
            if (roles == null || !roles.Any()) throw new ArgumentException("Roles cannot be null or empty.", nameof(roles));


            var jwtSettings = _config.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var timeExpires = jwtSettings["AccessTokenExpireMinutes"];

            // Kiểm tra cấu hình
            if (string.IsNullOrEmpty(key) || key.Length < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 characters long.");
            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT Issuer and Audience must be configured.");
            if (!int.TryParse(timeExpires, out var expiresMinutes))
                throw new InvalidOperationException("AccessTokenExpireMinutes must be a valid integer.");

            var keyBytes = Encoding.UTF8.GetBytes(key);

            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID duy nhất cho token
                new Claim(JwtRegisteredClaimNames.Iss, issuer)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(timeExpires)),
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
