using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Полето не може да остане празно")]
        [MaxLength(20)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Полето не може да остане празно")]
        [MaxLength(255)]
        public string Description { get; set; } = null!;

        public ICollection<News> News { get; set; } = new List<News>();

    }
}
