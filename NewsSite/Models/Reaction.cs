using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsSite.Models
{
    public class Reaction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(News))]
        public int NewsId { get; set; }
        public virtual News News { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        public bool IsLiked { get; set; }
    }
}
