using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models;
using NewsSite.Models.Pagination;
using System.Diagnostics;

namespace NewsSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private NewsSiteDbContext dbContext;

        public HomeController(ILogger<HomeController> logger, NewsSiteDbContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        public IActionResult Index(int? pageNumber, string term, int? categoryId = null)
        {
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            ViewBag.SelectedCategoryId = categoryId;

            var news = dbContext.News
                .Include(x => x.Category)
                .Include(x => x.Author)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                news = news.Where(h => h.Category.Id == categoryId.Value);
            }

            news = news.Where(x => x.Title.ToLower().Contains(term) || x.Description.ToLower().Contains(term))
                       .OrderByDescending(x => x.PublishedDate);

            if (!string.IsNullOrEmpty(term))
            {
                TempData["SearchResult"] = $"Резултати за търсенето \"{term}\"";
            }

            int pageSize = 20;
            return View(PagingModel<News>.Create(news.ToList(), pageNumber ?? 1, pageSize));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
