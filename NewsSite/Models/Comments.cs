using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static NewsSite.Enums.Enumerators;

namespace NewsSite.Models
{
    public class Comments
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Content { get; set; } = null!;

        public DateTime DateTime { get; set; }

        [ForeignKey(nameof(News))]
        public int NewsId { get; set; }

        public virtual News News { get; set; } = null!;

        [ForeignKey(nameof(Author))]
        public string? AuthorId { get; set; } = null!;

        public virtual User? Author { get; set; } = null!;

        public CommentsStatus Status { get; set; } = CommentsStatus.Pending;
    }
}
