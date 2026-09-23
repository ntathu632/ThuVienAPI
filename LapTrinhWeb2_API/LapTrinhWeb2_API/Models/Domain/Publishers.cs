using System.ComponentModel.DataAnnotations;

namespace LapTrinhWeb2_API.Models.Domain
{
    public class Publishers
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Books> Books { get; set; }
    }
}
