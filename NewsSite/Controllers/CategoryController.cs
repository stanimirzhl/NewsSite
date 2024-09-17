using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models;
using NewsSite.Models.Pagination;
using NewsSite.Models.ViewModels;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly NewsSiteDbContext dbContext;
        private readonly UserManager<User> userManager;
        public CategoryController(NewsSiteDbContext dbContext, UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        public IActionResult Index(int? pageNumber)
        {
            var categories = dbContext.Categories.ToList();
            return View(PagingModel<Category>.Create(categories, pageNumber ?? 1, 5));
        }
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Редактор")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryVM categoryVm)
        {

            if (ModelState.IsValid)
            {
                var category = new Category()
                {
                    Name = categoryVm.Name,
                    Description = categoryVm.Description,
                };
                dbContext.Categories.Add(category);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(categoryVm);
        }
        [Authorize(Roles = "Редактор")]
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            var category = dbContext.Categories.Find(id);
            var categoryVm = new CategoryVM()
            {
                Id = category.Id,
                Name = category.Name,
                Description= category.Description,
            };
            if (category == null)
            {
                return NotFound();
            }
            return View(categoryVm);
        }
        [Authorize(Roles = "Редактор")]
        [HttpPost]
        public async Task<IActionResult> Edit(CategoryVM categoryVm)
        {
            if (ModelState.IsValid)
            {
                var category = await dbContext.Categories.FindAsync(categoryVm.Id);

                category.Name = categoryVm.Name;
                category.Description = categoryVm.Description;

                dbContext.Categories.Update(category);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("Index"); 
            }

            return View(categoryVm);
        }
        [Authorize(Roles = "Редактор")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = dbContext.Categories.Find(id);
            var categoryVm = new CategoryVM()
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
            };
            if (category == null)
            {
                return NotFound();
            }
            return View(categoryVm);
        }
        [Authorize(Roles = "Редактор")]
        [HttpPost]
        public async Task<IActionResult> Delete(CategoryVM categoryVm)
        {
                var category = await dbContext.Categories.Include(x => x.News).FirstOrDefaultAsync(c => c.Id == categoryVm.Id);
                dbContext.Categories.Remove(category);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Details(int id, int? pageNumber)
        {
            int pageSize = 20;
            //term = string.IsNullOrEmpty(term) ? "" : term.ToLower();
            var category = dbContext.Categories.FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            var newsQuery = dbContext.News.Where(n => n.CategoryId == id)
                                         .OrderByDescending(n => n.PublishedDate)
                                         .ToList();
            var paginatedNews = PagingModel<News>.Create(newsQuery, pageNumber ?? 1, pageSize);
            var viewModel = new CategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                PaginatedNews = paginatedNews
            };

            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubscribeToCategory(int categoryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);

            var category = await dbContext.Categories.FindAsync(categoryId);
            if (category != null)
            {
                if (!user.SubscribedCategories.Contains(category))
                {
                    user.SubscribedCategories.Add(category);
                    await userManager.UpdateAsync(user);
                    TempData["Success"] = "Успешно се абонирахте за категорията.";
                }
                else
                {
                    user.SubscribedCategories.Remove(category);
                    await userManager.UpdateAsync(user);
                    TempData["Success"] = "Успешно се отбонирахте от категорията.";
                }
            }

            return RedirectToAction("Index");
        }
    }
}
