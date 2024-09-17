using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models;
using NewsSite.Models.Pagination;
using NewsSite.Models.ViewModels;
using NewsSite.Services;
using System.Drawing.Printing;
using System.IO.Compression;
using System.Security.Claims;
using static NewsSite.Enums.Enumerators;

namespace NewsSite.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsSiteDbContext dbContext;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;
        public NewsController(NewsSiteDbContext dbContext, IWebHostEnvironment hostingEnvironment, UserManager<User> userManager, IEmailService emailService)
        {
            this.dbContext = dbContext;
            this.hostingEnvironment = hostingEnvironment;
            this.userManager = userManager;
            this.emailService = emailService;
        }
        public IActionResult Index()
        {
            var news = dbContext.News.ToList();
            return View(news);
        }
        [Authorize(Roles = "Редактор")]
        public IActionResult Create()
        {
            var categories = dbContext.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Редактор")]
        public async Task<IActionResult> Create(NewsVM newsVM, IFormFile? mainImage, List<IFormFile>? files)
        {
            ModelState.Remove("MainImage");
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(dbContext.Categories.ToList(), "Id", "Name");
                ModelState.AddModelError("MainImage", "Моля добавете снимка");
                return View(newsVM);
            }
            if (mainImage != null)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                News news = new News
                {
                    Title = newsVM.Title,
                    Introduction = newsVM.Introduction,
                    Description = newsVM.Description,
                    PublishedDate = newsVM.TimeOfCreation,
                    CategoryId = newsVM.CategoryId,
                    AuthorId = currentUserId
                };
                var mainImageName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                var mainImagePath = Path.Combine(hostingEnvironment.WebRootPath, @"Images\MainImages", mainImageName);

                using (var stream = new FileStream(mainImagePath, FileMode.Create))
                {
                    await mainImage.CopyToAsync(stream);
                }
                news.MainImage = @"\Images\MainImages\" + mainImageName;

                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(hostingEnvironment.WebRootPath, @"Images\MainImages", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        news.AdditionalImages.Add(@"\Images\Additional\" + fileName);
                    }
                }
                dbContext.News.Add(news);
                await dbContext.SaveChangesAsync();
                if (news.CategoryId != null)
                {
                    var subscribers = userManager.Users.Where(u => u.SubscribedCategories.Any(c => c.Id == news.CategoryId)).ToList();

                    foreach (var user in subscribers)
                    {
                        var subject = "Ново известие";
                        var message = $@"
                        <p>Здравейте, в категория {dbContext.Categories.Find(news.CategoryId).Name} е качена нова новина.</p>";
                        await emailService.SendEmailAsync(user.Email, subject, message);
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Categories = new SelectList(dbContext.Categories.ToList(), "Id", "Name");
            ModelState.AddModelError("MainImage", "Моля добавете снимка");
            return View(newsVM);


        }
        [Authorize(Roles = "Редактор")]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var news = dbContext.News.Find(id);
            if (news == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(dbContext.Categories.ToList(), "Id", "Name");
            var newsVm = new NewsVM
            {
                Id = news.Id,
                Title = news.Title,
                Introduction = news.Introduction,
                Description = news.Description,
                MainImage = news.MainImage,
                TimeOfCreation = news.PublishedDate,
                CategoryId = news.CategoryId,
                AdditionalImages = news.AdditionalImages
            };
            return View(newsVm);
        }
        [Authorize(Roles = "Редактор")]
        [HttpPost]
        public async Task<IActionResult> Edit(NewsVM newsVM, IFormFile? mainImage, List<IFormFile>? files)
        {
            ModelState.Remove("MainImage");
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(dbContext.Categories.ToList(), "Id", "Name");
                return View(newsVM);
            }

            var news = await dbContext.News.FindAsync(newsVM.Id);

            if (!news.Title.Contains("редактирано"))
            {
                news.Title = newsVM.Title + "(редактирано)";
            }
            news.Title = newsVM.Title;
            news.Introduction = newsVM.Introduction;
            news.Description = newsVM.Description;
            news.PublishedDate = newsVM.TimeOfCreation;
            news.CategoryId = newsVM.CategoryId;

            if (mainImage != null)
            {
                var newImageFileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                var newImagePath = Path.Combine(hostingEnvironment.WebRootPath, @"Images\MainImages", newImageFileName);
                var oldImagePath = Path.Combine(hostingEnvironment.WebRootPath, news.MainImage.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                using (var stream = new FileStream(newImagePath, FileMode.Create))
                {
                    await mainImage.CopyToAsync(stream);
                }
                news.MainImage = @"\Images\MainImages\" + newImageFileName;
            }
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(hostingEnvironment.WebRootPath, @"Images\Additional", fileName);
                    if (news.AdditionalImages.Any())
                    {
                        var oldImgList = news.AdditionalImages.ToList();
                        foreach (var oldimg in oldImgList)
                        {
                            var oldImagePath = Path.Combine(hostingEnvironment.WebRootPath, oldimg.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            news.AdditionalImages.Remove(oldimg);
                        }
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    news.AdditionalImages.Add(@"\Images\Additional\" + fileName);
                }
            }

            dbContext.Update(news);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "Редактор")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var news = dbContext.News.Find(id);
            if (news == null)
            {
                return NotFound();
            }
            var newsVM = new NewsVM()
            {
                Id = news.Id,
                Title = news.Title,
                Introduction = news.Introduction,
                Description = news.Description,
                TimeOfCreation = news.PublishedDate,
                MainImage = news.MainImage,
                AdditionalImages = news.AdditionalImages

            };
            return View(newsVM);
        }
        [HttpPost]
        [Authorize(Roles = "Редактор")]
        public async Task<IActionResult> Delete(NewsVM newsVM)
        {
            var news = await dbContext.News.Include(x => x.Comments).FirstOrDefaultAsync(c => c.Id == newsVM.Id);
            var mainImgPath = Path.Combine(hostingEnvironment.WebRootPath, news.MainImage.TrimStart('\\'));
            if (System.IO.File.Exists(mainImgPath)) System.IO.File.Delete(mainImgPath);
            if (news.AdditionalImages.Count > 0)
            {
                while (news.AdditionalImages.Any())
                {
                    var addImg = news.AdditionalImages.First();
                    var addImgPath = Path.Combine(hostingEnvironment.WebRootPath, addImg.TrimStart('\\'));
                    if (System.IO.File.Exists(addImgPath))
                    {
                        System.IO.File.Delete(addImgPath);
                    }
                    news.AdditionalImages.Remove(addImg);
                }


            }
            dbContext.News.Remove(news);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        public IActionResult Details(int? id, int? pageNumber = 1)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var news = dbContext.News.Include(x => x.Reactions).Include(n => n.Category).Include(x => x.Comments).ThenInclude(x => x.Author)
                .Include(x => x.Author).FirstOrDefault(x => x.Id == id);

            var approvedComments = news.Comments
            .Where(c => c.Status == CommentsStatus.Approved)
            .OrderBy(c => c.DateTime)
            .ToList();

            var paginatedComments = PagingModel<Comments>.Create(approvedComments, pageNumber ?? 1, 50);

            var viewModel = new NewsDetailsVM
            {
                News = news,
                PagedComments = paginatedComments
            };
            return View(viewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Потребител")]
        public async Task<IActionResult> AddComment(int newsId, string content, string title)
        {
            var comment = new Comments
            {
                Content = content,
                Title = title,
                DateTime = DateTime.Now,
                NewsId = newsId,
                AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            TempData["AddedComment"] = "Коментарът ви е изпратен за проверка от нашите модератори";
            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = newsId });
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveNews(int newsId)
        {
            var news = await dbContext.News.FirstOrDefaultAsync(x => x.Id == newsId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);
            if (user.SavedNews.Contains(news))
            {
                user.SavedNews.Remove(news);
                await userManager.UpdateAsync(user);
                return RedirectToAction("Details", new { id = newsId });
            }

            user.SavedNews.Add(news);
            await userManager.UpdateAsync(user);
            return RedirectToAction("Details", new { id = newsId });
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> React(int newsId, bool isLike)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var news = await dbContext.News.Include(x => x.Reactions).FirstOrDefaultAsync(n => n.Id == newsId);

            var hasUserReacted = news.Reactions.FirstOrDefault(x => x.UserId == userId);

            if (hasUserReacted != null)
            {
                hasUserReacted.IsLiked = isLike;
            }
            else
            {
                news.Reactions.Add(new Reaction
                {
                    UserId = userId,
                    NewsId = newsId,
                    IsLiked = isLike
                });
            }
            await dbContext.SaveChangesAsync();

            return RedirectToAction("Details", new { id = newsId });
        }
    }
}



