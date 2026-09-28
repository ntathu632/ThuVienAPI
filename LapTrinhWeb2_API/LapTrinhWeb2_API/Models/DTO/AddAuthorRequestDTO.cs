using System.ComponentModel.DataAnnotations;

namespace LapTrinhWeb2_API.Models.DTO
{
    public class AddAuthorRequestDTO
    {
        [Required]
        [MinLength(3)]
        public string FullName { set; get; }
    }
}
