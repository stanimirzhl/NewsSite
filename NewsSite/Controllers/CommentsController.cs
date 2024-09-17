using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models;
using NewsSite.Services;
using static NewsSite.Enums.Enumerators;

namespace NewsSite.Controllers
{
    public class CommentsController : Controller
    {
        private NewsSiteDbContext dbContext;
        private UserManager<User> userManager;
        private IEmailService emailService;
        public CommentsController(NewsSiteDbContext dbContext, UserManager<User> userManager, IEmailService emailService)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.emailService = emailService;
        }
        [Authorize(Roles = "Модератор")]
        public IActionResult Index()
        {
            var comments = dbContext.Comments.Where(x => x.Status == CommentsStatus.Pending).Include(x => x.News).Include(x => x.Author).ToList();
            return View(comments);
        }
        [HttpPost]
        [Authorize(Roles = "Модератор")]
        public async Task<IActionResult> Approve(int commentId)
        {
            var comment = await dbContext.Comments.Include(x=> x.News).FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment != null)
            {
                comment.Status = CommentsStatus.Approved;
                dbContext.Comments.Update(comment);
                await dbContext.SaveChangesAsync();

                var commenters = dbContext.Comments
                    .Where(x => x.NewsId == comment.NewsId && x.AuthorId != comment.AuthorId && x.AuthorId != null).Select(x => x.AuthorId).Distinct().ToList();

                var usersToNotify = userManager.Users.Where(x => commenters.Contains(x.Id)).ToList();
                foreach (var user in usersToNotify)
                {
                    var subject = "Ново известие";
                    var message = $@"
                    <p>Здравейте, под новината {comment.News.Title} има нов коментар.</p>";
                    await emailService.SendEmailAsync(user.Email, subject, message);
                }
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = "Модератор")]
        public async Task<IActionResult> Reject(int commentId)
        {
            var comment = await dbContext.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment != null)
            {
                dbContext.Comments.Remove(comment);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
