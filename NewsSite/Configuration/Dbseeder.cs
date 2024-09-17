using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Data;
using NewsSite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static NewsSite.Enums.Enumerators;

public static class DbSeeder
{
    public static async Task SeedDatabase(IServiceProvider serviceProvider, bool isDevelopment)
    {
        var context = serviceProvider.GetRequiredService<NewsSiteDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();
        var roles = new[] { "Администратор", "Редактор", "Модератор", "Потребител" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (isDevelopment)
        {
            await SeedDevelopmentData(context, userManager, roleManager);
        }
        else
        {
            await SeedProductionData(context, userManager, roleManager);
        }
    }

    private static async Task SeedDevelopmentData(NewsSiteDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        var passwordHasher = new PasswordHasher<User>();
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                UserName = "Admin",
                Email = "admin@gmail.com",
                FirstName = "Admin",
                LastName = "User",
                Status = UserStatus.Approved
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "adminPass123");

            await userManager.CreateAsync(adminUser, "adminPass123");
            await userManager.AddToRoleAsync(adminUser, "Администратор");

            var editorUser = new User
            {
                UserName = "Editor",
                Email = "editor@gmail.com",
                FirstName = "Editor",
                LastName = "User",
                Status = UserStatus.Approved
            };
            editorUser.PasswordHash = passwordHasher.HashPassword(editorUser, "editorPass123");
            await userManager.CreateAsync(editorUser, "editorPass123!");
            await userManager.AddToRoleAsync(editorUser, "Редактор");
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(new[]
            {
                new Category { Name = "Спорт", Description = "Последните новини за спортни събития и отбори." },
                new Category { Name = "Технологии", Description = "Новини за света на информационните технологии и новите иновации." },
            });
            await context.SaveChangesAsync();
        }

        if (!context.News.Any())
        {
            var categories = context.Categories.ToList();
            var sampleNews = new[]
            {
                new News
                {
                    Title = "OpenAI се подготвя да постигне оценка от 150 милиарда долара",
                    Introduction = "Компанията събира нови средства и е готова за разширяване",
                    Description = "Американската технологична компания OpenAI преговаря за нов рунд за набиране на средства, който би оценил компанията на 150 милиарда долара, съобщава Ройтерс, цитирайки Блумбърг." +
                    " Този ход, обявен късно вчера, ще укрепи позицията на OpenAI като една от най-големите стартиращи компании в света, отбелязва БТА." +
                    "\r\n\r\nСъздателят на популярния чатбот ChatGPT планира да набере 6,5 милиарда долара от инвеститори и още 5 милиарда долара заеми от банки под формата на револвиращ кредит, според източници, запознати със ситуацията." +
                    "\r\n\r\nНовата оценка на OpenAI ще бъде с 74% по-висока от предишните 86 милиарда долара, които компанията получи по-рано тази година.\r\n\r\nКомпанията не отговори на искането на Ройтерс за коментар." +
                    "\r\n\r\nАмериканската фирма за рисков капитал Thrive Capital, която ще ръководи финансирането, също отказа коментар.\r\n\r\nChatGPT превърна OpenAI в една от водещите компании в сферата на изкуствения интелект." +
                    "\r\n\r\nПлатформата за финансови услуги Forge Global Holdings добави OpenAI към списъка си със стартиращи компании от \"Великолепната седморка\", която включва водещи американски технологични компании като Microsoft, Apple, Google (Alphabet), Tesla и други." +
                    "\r\n\r\nТази последна финансова инжекция ще позволи на OpenAI да остане непублична за по-дълго време, като повечето успешни стартиращи компании избягват публичното предлагане поради регулаторните разходи и нестабилността на фондовите пазари.",
                    PublishedDate = DateTime.Now,
                    MainImage = "https://www.gettyimages.com/detail/news-photo/openai-ceo-sam-altman-speaks-during-the-openai-devday-event-news-photo/1778704898",
                    CategoryId = categories.FirstOrDefault(x => x.Name == "Технологии").Id,
                    Category = categories.FirstOrDefault(x => x.Name=="Технологии"),
                    Author = await userManager.FindByEmailAsync("admin@gmail.com"),
                    AuthorId = context.Users.FirstOrDefault(x => x.UserName == "Admin").Id


                },
                new News
                {
                    Title = "Карлос Насар коментира скандалите и бъдещето на българските щанги",
                    Introduction = "Карлос Насар: Скандалите заплашват бъдещето на щангите, но ще продължа да се боря",
                    Description = "Олимпийският шампион и световен рекордьор по щанги Карлос Насар получи наградата за спортист на месец август. Насар, който спечели златен медал за България на игрите в Париж, коментира актуалните скандали в щангите, които продължават да предизвикват внимание." +
                    "\r\n\r\nНасар заяви, че е бил принуден, заедно с колегите си, да превежда 30% от приходите си към федерацията. Президентът на Българската федерация по щанги, Антон Коджабашев, призова Насар да премине полиграфски тест, за да потвърди твърденията си." +
                    "\r\n\r\nНа награждаването Насар сподели: \"Ако въпросите са зададени правилно, Коджабашев няма да успее да премине теста. Но смятам, че има по-важни въпроси. Тренирам по два пъти на ден и спазвам режим. Проблемите трябва да бъдат обсъдени и решени. Тези 30% отиват за моята подготовка и, когато ги дам, се лишавам от част от необходимото. Препоръчвам на Коджабашев да бъде честен към себе си и към другите." +
                    "\r\n\r\nРаботим усилено, защото обичаме спорта. Клубовете с по 20-30 деца не могат да се справят без средства. Не трябва да се поддаваме на грешни решения, които могат да застрашат бъдещето на българските щанги.\r\n\r\nПризовавам за хора с визия и ангажимент в нашата федерация. Нямам нищо против Стефан Ботев, но той трябва да уточни намеренията си. \"Ще видим какво можем да направим\" вече не е достатъчно." +
                    "\r\n\r\nИмам още какво да покажа и искам да стана отново олимпийски шампион. Фокусът ми е върху Световното първенство в Бахрейн. Ако продължи бездействието на федерацията, може да загубим лиценза си и да не участваме. Моите критики към Коджабашев са професионални. Искам федерацията ни да бъде водена от хора, които обичат и разбират спорта.",
                    PublishedDate = DateTime.Now,
                    MainImage = "https://www.gettyimages.com/detail/news-photo/karlos-may-nasar-of-team-bulgaria-performs-a-clean-and-jerk-news-photo/2166046873",
                    Category = categories.FirstOrDefault(c => c.Name == "Спорт"),
                    CategoryId = categories.FirstOrDefault(x => x.Name == "Спорт").Id,
                    Author = await userManager.FindByEmailAsync("editor@example.com"),
                    AuthorId = context.Users.FirstOrDefault(x => x.UserName == "Editor").Id
                }
            };
            context.News.AddRange(sampleNews);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedProductionData(NewsSiteDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        var passwordHasher = new PasswordHasher<User>();
        var adminUser = new User
        {
            UserName = "Admin",
            Email = "admin@gmail.com",
            FirstName = "Admin",
            LastName = "User",
            Status = UserStatus.Approved
        };

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "adminPass123");

        await userManager.CreateAsync(adminUser, "adminPass123");
        await userManager.AddToRoleAsync(adminUser, "Администратор");
    }
}
