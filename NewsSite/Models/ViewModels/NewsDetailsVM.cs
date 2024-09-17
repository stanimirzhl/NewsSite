using NewsSite.Models.Pagination;

namespace NewsSite.Models.ViewModels
{
    public class NewsDetailsVM
    {
        public News News { get; set; } 
        public PagingModel<Comments> PagedComments { get; set; } 
    }
}
