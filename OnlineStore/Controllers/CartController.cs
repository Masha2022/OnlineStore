using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using OnlineStore.Utility;

namespace OnlineStore.Controllers;
// Этот контроллер используется для работы с корзиной покупок
[Authorize] // Только авторизованные пользователи могут получить доступ
public class CartController : Controller
{
    private readonly ApplicationDbContext _db; // Ссылка на контекст базы данных

    [BindProperty] // Атрибут для автоматического связывания данных формы с этой моделью
    public ProductUserVM ProductUserVm { get; set; }

    // Конструктор для инициализации контекста базы данных
    public CartController(ApplicationDbContext dbContext)
    {
        _db = dbContext;
    }

    // Метод для отображения содержимого корзины
    public IActionResult Index()
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        
        // Проверка наличия товаров в корзине в сессии

        var shoppingCarts = HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart);
        if (shoppingCarts != null && shoppingCarts.Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        List<int> productInCart = shoppingCartList.Select(p => p.ProductId).ToList();
        IEnumerable<Product> products = _db.Product.Where(p => productInCart.Contains(p.Id));

        return View(products);
    }

    // POST-запрос для перехода к итоговой странице
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Index")]
    public IActionResult IndexPost()
    {
        return RedirectToAction(nameof(Summary));
    }
    
    // Итоговая страница корзины с товарами
    public IActionResult Summary()
    {
        // Извлечение идентификатора текущего пользователя
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //тоже самое
        //var userId = User.FindFirstValue(ClaimTypes.Value);
        
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        List<int> productInCart = shoppingCartList.Select(p => p.ProductId).ToList();
        IEnumerable<Product> products = _db.Product.Where(p => productInCart.Contains(p.Id));

        // Инициализация ViewModel
        ProductUserVm = new ProductUserVM()
        {
            ApplicationUser = _db.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value),
            ProductList = products
        };
        
        return View(ProductUserVm);
    }

    // Удаление товара из корзины
    public IActionResult Remove(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        // Удаление выбранного товара
        shoppingCartList.Remove(shoppingCartList.FirstOrDefault(p => p.ProductId == id));

        // Обновление корзины в сессии
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

        return RedirectToAction(nameof(Index));
    }
}
