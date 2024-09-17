using NewsSite.Data;
using NewsSite.Models;

namespace NewsSite.Services
{
        public class CategoryService
        {
            private readonly NewsSiteDbContext _context;

            public CategoryService(NewsSiteDbContext context)
            {
                _context = context;
            }

            public List<Category> GetAllCategories()
            {
                return _context.Categories.ToList();
            }
        }
}
