using System.ComponentModel.DataAnnotations;

namespace library_web.Models.DTO
{
    public class loginDTO
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class loginResponseDTO
    {
        public string JwtToken { get; set; } = string.Empty;
    }
}
