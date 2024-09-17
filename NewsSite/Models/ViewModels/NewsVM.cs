using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class NewsVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Introduction { get; set; } = null!;

        [Required(ErrorMessage = "Полето не може да остане празно")]
        public string Description { get; set; } = null!;

        public DateTime TimeOfCreation { get; set; } = DateTime.Now;

        public string? MainImage { get; set; } = null!;

        public List<string> AdditionalImages { get; set; } = new List<string>();

        public int? CategoryId { get; set; }
    }
}
