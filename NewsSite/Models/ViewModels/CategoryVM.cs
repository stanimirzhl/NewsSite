using NewsSite.Models.Pagination;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class CategoryVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Description { get; set; } = null!;

        public PagingModel<News>? PaginatedNews { get; set; }
    }
}
