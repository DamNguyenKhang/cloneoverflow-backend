using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }

}
