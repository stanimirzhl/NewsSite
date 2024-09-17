using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static NewsSite.Enums.Enumerators;

namespace NewsSite.Models
{
    public class User : IdentityUser
    {

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string LastName { get; set; }

        public virtual ICollection<News>? News { get; set; } = new List<News>();

        public virtual ICollection<Comments>? Comments { get; set; } = new List<Comments>();

        public UserStatus Status { get; set; } = UserStatus.Pending;

        public List<Category> SubscribedCategories { get; set; } = new List<Category>();

        public List<News> SavedNews { get; set; } = new List<News>();

        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
