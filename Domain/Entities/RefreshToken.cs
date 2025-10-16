using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string RefreshTokenString { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ReplacedByToken { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
