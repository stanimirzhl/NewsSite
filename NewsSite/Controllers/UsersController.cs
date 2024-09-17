using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models;
using NewsSite.Models.Pagination;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing.Printing;
using NewsSite.Models.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static NewsSite.Enums.Enumerators;
using NewsSite.Services;

namespace NewsSite.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<User> signInManager;
        private readonly IEmailService emailService;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IEmailService emailService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
        }
        public IActionResult Index(int? pageNumber)
        {
            var users = userManager.Users.ToList();
            return View(PagingModel<User>.Create(users, pageNumber ?? 1, 5));
        }
        [Authorize(Roles = "Администратор")]
        public IActionResult Create()
        {
            ViewBag.Roles = roleManager.Roles
               .Select(role => new SelectListItem
               {
                   Value = role.Id,
                   Text = role.Name
               }).ToList();

            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Create(RegisterVM model, List<string> selectedRoles)
        {
            if (ModelState.IsValid)
            {
                if (await userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Този електронен адрес вече е зает.");
                    ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                    {
                        Value = role.Id,
                        Text = role.Name
                    }
                    )
                    .ToList();
                    return View();
                }
                if (await userManager.FindByNameAsync(model.UserName) != null)
                {
                    ModelState.AddModelError("userName", "Това потребителско име вече е заето.");
                    ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                    {
                        Value = role.Id,
                        Text = role.Name
                    }
                    )
                    .ToList();
                    return View();
                }
                var user = new User()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (selectedRoles != null && selectedRoles.Any())
                    {
                        var roleNames = (await roleManager.Roles
                        .Where(role => selectedRoles.Contains(role.Id)).ToListAsync())
                        .Select(role => role.Name).ToList();
                        var roleResult = await userManager.AddToRolesAsync(user, roleNames);
                    }
                    return RedirectToAction("Index", "Users");
                }
            }
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Edit(string? id)
        {
            var user = await userManager.FindByIdAsync(id);
            var editUser = new EditUserVM
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Id = id
            };
            ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
            {
                Value = role.Id,
                Text = role.Name
            })
            .ToList();
            return View(editUser);
        }
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Edit(EditUserVM model, List<string> selectedRoles)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }
            var existingEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null && existingEmail.Id != user.Id)
            {
                ModelState.AddModelError("Email", "Този електронен адрес вече е зает.");
                ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                }
                    )
                    .ToList();
                return View(model);
            }
            var existingUserName = await userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null && existingUserName.Id != user.Id)
            {
                ModelState.AddModelError("UserName", "Това потребителско име вече е заето.");
                ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                }
                    )
                    .ToList();
                return View(model);
            }
            if (model.Password != null && model.ConfirmPassword == null)
            {
                ModelState.AddModelError(string.Empty, "Паролата не може да остане празна.");
                ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                }
                    )
                    .ToList();
                return View(model);
            }
            if (model.Password == null && model.ConfirmPassword != null)
            {
                ModelState.AddModelError(string.Empty, "Паролата не може да остане празна.");
                ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                }
                    )
                    .ToList();
                return View(model);
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.ConfirmPassword))
            {
                if (!IsPassValid(model.Password) || !IsPassValid(model.ConfirmPassword))
                {
                    ModelState.AddModelError(string.Empty, "Паролата трябва да съдържа поне една главна буква, една малка, число и специален символ, като допустимата дължина е от 5 до 25 символа.");
                    ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                    {
                        Value = role.Id,
                        Text = role.Name
                    }
                    )
                    .ToList();
                    return View(model);
                }
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Паролите не съвпадат.");
                    ViewBag.Roles = roleManager.Roles.Select(role => new SelectListItem
                    {
                        Value = role.Id,
                        Text = role.Name
                    }
                    )
                    .ToList();
                    return View(model);
                }
                var subject = "Вашите данни бяха променени";
                var message = $@"
                <p>Здравейте, вашите данни бяха редактирани от нашите администратори, долу ще се появят новите ви данни за вход.</p>
                <p><strong>Потребителско име:</strong> {user.UserName}</p>
                <p><strong>Име:</strong> {user.FirstName}.</p>
                <p><strong>Фамилия:</strong> {user.LastName}.</p>
                <p><strong>Електронна поща:</strong> {user.Email}.</p>
                <p><strong>Нова парола:</strong> {model.Password}</p>
                ";

                await emailService.SendEmailAsync(user.Email, subject, message);
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, resetToken, model.Password);
            }
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (selectedRoles != null && selectedRoles.Any())
                {
                    var currentRoles = await userManager.GetRolesAsync(user);

                    var roleNames = (await roleManager.Roles
                        .Where(role => selectedRoles.Contains(role.Id)).ToListAsync())
                        .Select(role => role.Name).ToList();
                    var rolesToAdd = roleNames.Except(currentRoles).ToList();
                    var rolesToRemove = currentRoles.Except(roleNames).ToList();
                    await userManager.AddToRolesAsync(user, rolesToAdd);
                    await userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
                return RedirectToAction("Index", "Users");
            }
            return RedirectToAction("Index", "Users");
        }
        [Authorize(Roles = "Администратор")]
        public IActionResult DeleteUser(string? id)
        {
            var user = userManager.FindByIdAsync(id);
            return View(id);
        }
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Delete(string? id)
        {
            var user = await userManager.FindByIdAsync(id);
            await userManager.DeleteAsync(user);
            return RedirectToAction("Index", "Users");
        }
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Approve(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Status = UserStatus.Approved;
                await userManager.AddToRoleAsync(user, "Потребител");
                await userManager.UpdateAsync(user);
                var subject = "Вие бяхте одобрени";
                var message = $@"
                <h2>Поздравления!</h2>
                <p>Вашето заявление за участие в нашата платформа беше прието.</p>
                <p><strong>Потребителско име:</strong> {user.UserName}</p>
                <p><strong>Име:</strong> {user.FirstName}.</p>
                <p><strong>Фамилия:</strong> {user.LastName}.</p>
                <p><strong>Парола:</strong> Използвайте създадената от вас парола, за да влезете в акаунта си.</p>
                <p>Вече имате възможността да добавяте коментари, реагирате на новини и да се абонирате за избрани от вас категории.</p>
                ";

                await emailService.SendEmailAsync(user.Email, subject, message);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Reject(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var subject = "Вие не бяхте одобрени";
                var message = $@"
                <p>За съжаление вашето заявление за участие в нашата платформа беше отхвърлено.</p>
                <p><strong>Потребителско име:</strong> {user.UserName}</p>
                <p><strong>Име:</strong> {user.FirstName}.</p>
                <p><strong>Фамилия:</strong> {user.LastName}.</p>
                ";
                await emailService.SendEmailAsync(user.Email, subject, message);
                await userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }
        private bool IsPassValid(string password)
        {
            string regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{5,25}$";
            return Regex.IsMatch(password, regex);
        }
    }
}
