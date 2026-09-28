using LapTrinhWeb2_API.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace LapTrinhWeb2_API.Models.DTO
{
    public class addBookRequestDTO
    {
        [Required]
        [MinLength(10)]
        [RegularExpression(@"^[a-zA-Z0-9À-ỹ\s]+$", ErrorMessage = "Title không được chứa ký tự đặc biệt")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        [Range(0, 5, ErrorMessage ="From 0 to 5")]
        public int? Rate { get; set; }
        public string? Genre { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; }
        public int PublisherId { get; set; }
        public List<int> AuthorIds { get; set; }
    }
}
