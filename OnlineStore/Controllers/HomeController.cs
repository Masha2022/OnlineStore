using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using OnlineStore.Utility;

namespace OnlineStore.Controllers;
// Контроллер для главной страницы интернет-магазина и некоторых связанных действий.
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger; // Сервис для логирования
    private ApplicationDbContext _dbContext; // Ссылка на контекст базы данных для доступа к данным

    // Конструктор с зависимостями от сервиса логирования и контекста базы данных
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Отображение главной страницы магазина
    public IActionResult Index()
    {
        HomeVM homeVm = new HomeVM()
        {
            Products = _dbContext.Product.Include(p => p.Category).Include(p => p.ApplicationType), // Получение списка продуктов с привязкой к категориям и типам приложений
            Categories = _dbContext.Category // Получение списка категорий
        };
        return View(homeVm);
    }

    // Отображение страницы "Политика конфиденциальности"
    public IActionResult Privacy()
    {
        return View();
    }

    // Отображение страницы ошибки
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Отображение страницы деталей продукта
    public IActionResult Details(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

        // Получение списка продуктов из корзины покупок из сессии
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }
        
        DetailsVM detailsVm = new DetailsVM()
        {
            Product = _dbContext.Product.Include(p => p.Category).
                Include(p => p.ApplicationType).
                Where(p => p.Id == id)
                .FirstOrDefault(), // Получение информации о продукте по ID
            ExistInCart = false
        };

        // Проверка, находится ли продукт в корзине
        foreach (var item in shoppingCartList)
        {
            if (item.ProductId == id)
            {
                detailsVm.ExistInCart = true;
            }
        }
        return View(detailsVm);
    }
    
    // Добавление продукта в корзину покупок
    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        // Получение списка продуктов из корзины покупок из сессии
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        shoppingCartList.Add(new ShoppingCart { ProductId = id }); // Добавление продукта в корзину
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList); // Сохранение корзины в сессии
        return RedirectToAction(nameof(Index)); // Перенаправление на главную страницу
    }

    // Удаление продукта из корзины покупок
    public IActionResult RemoveFromCart(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        // Получение списка продуктов из корзины покупок из сессии
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        // Поиск и удаление продукта из корзины
        var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
        if (itemToRemove != null)
        {
            shoppingCartList.Remove(itemToRemove);
        }
        
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList); // Сохранение изменений корзины в сессии
        return RedirectToAction(nameof(Index)); // Перенаправление на главную страницу
    }
}