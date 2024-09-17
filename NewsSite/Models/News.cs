using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsSite.Models
{
    public class News
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Title { get; set; } = null!;

        [MaxLength(300)]
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Introduction { get; set; } = null!;
        
        [MaxLength(4000)]
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Description { get; set; } = null!;

        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

        [ForeignKey(nameof(Author))]
        public string? AuthorId { get; set; } = null!;

        public virtual User? Author { get; set; } = null!;

        public DateTime PublishedDate { get; set; }

        public string MainImage { get; set; } = null!;

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public ICollection<Comments> Comments { get; set; } = new List<Comments>();

        [ForeignKey(nameof(Category))]
        public int? CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
    }
}
