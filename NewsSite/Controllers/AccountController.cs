using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models;
using NewsSite.Models.ViewModels;
using NuGet.Protocol;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;

namespace NewsSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly NewsSiteDbContext dbContext;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, NewsSiteDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userNameExists = await userManager.FindByNameAsync(model.UserName) != null;
            var emailExists = await userManager.FindByEmailAsync(model.Email) != null;

            if (userNameExists)
            {
                ModelState.AddModelError(string.Empty, "Потребителското име е заето.");
                return View(model);
            }

            if (emailExists)
            {
                ModelState.AddModelError(string.Empty, "Вече има потребител с този имейл.");
                return View(model);
            }
            var user = new User()
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            TempData["Register"] = "Регистрацията ви ще бъде разгледана от нашите администратори и ще получите имейл на дадената електронна поща ако сте били одобрени";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var model = new LoginVM
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Потребителя не съществува");
                return View(model);
            }
            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            ModelState.AddModelError(string.Empty, "Неуспешен вход");
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [Authorize]
        public async Task<IActionResult> Profile(string section)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await dbContext.Users.Include(x => x.SavedNews).FirstOrDefaultAsync(x => x.Id == currentUserId);
            var model = new SettingsVM
            {
                Section = section,
                ProfileSettings = new ProfileSettingsVM(){
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                },
                ChangePassword = new ChangePasswordVM(),
                SavedNews = user.SavedNews
            };
            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> UpdateProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(currentUserId);
            var settingsVM = new ProfileSettingsVM
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
            var settings = new SettingsVM()
            {
                ProfileSettings = settingsVM
            };
            return PartialView("_SettingsPartial", settings);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(SettingsVM model)
        {

            var user = await userManager.GetUserAsync(User);
            user.UserName = model.ProfileSettings.UserName;
            user.FirstName = model.ProfileSettings.FirstName;
            user.LastName = model.ProfileSettings.LastName;
            user.Email = model.ProfileSettings.Email;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Профила е успешно редактиран";
            }
            return RedirectToAction("Profile", new {section = "profile" });
        }
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            var settingsVM = new SettingsVM();
            return PartialView("_ChangePasswordPartial", settingsVM);
        }
        [Authorize]
        public async Task<IActionResult> ChangePassowrd(SettingsVM model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(currentUserId);
            var checkPass = await userManager.CheckPasswordAsync(user, model.ChangePassword.OldPassword);
            if (!checkPass)
            {
                TempData["Error"] = "Старата парола не е правилна";
                return RedirectToAction("Profile", new { section = "changePassword" });
            }
            if(!IsPassValid(model.ChangePassword.NewPassword) && !IsPassValid(model.ChangePassword.NewPasswordConfirm))
            {
                TempData["Error"] = "Паролата трябва да съдържа поне една главна буква, една малка, число и специален символ.";
                return RedirectToAction("Profile", new { section = "changePassword" });
            }
            var result = await userManager.ChangePasswordAsync(user,model.ChangePassword.OldPassword,model.ChangePassword.NewPassword);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Паролата бе успешно променена";
                return RedirectToAction("Profile", new { section = "changePassword" });
            }
            return RedirectToAction("Profile", new { section = "changePassword" });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SavedNews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await dbContext.Users.Include(x => x.SavedNews).FirstOrDefaultAsync(x => x.Id == userId);
            var model = new SettingsVM
            {
                SavedNews = user.SavedNews
            };

            return PartialView("_SavedNewsPartial", model);
        }
        private bool IsPassValid(string password)
        {
            string regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{5,25}$";
            return Regex.IsMatch(password, regex);
        }
    }
}
