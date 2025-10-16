using Microsoft.AspNetCore.Identity;

namespace Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; } 
        public string? LastName { get; set; }    
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdateddAt { get; set; } = DateTime.UtcNow;

        // Có thể thêm các thuộc tính khác tùy nhu cầu
        public string FullName => $"{FirstName} {LastName}";
    }
}
