using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
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
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEmailSender _emailSender;
    [BindProperty] // Атрибут для автоматического связывания данных формы с этой моделью
    public ProductUserVM ProductUserVm { get; set; }

    // Конструктор для инициализации контекста базы данных
    public CartController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
    {
        _db = dbContext;
        _webHostEnvironment = webHostEnvironment;
        _emailSender = emailSender;
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

    // Итоговая страница корзины с товарами - get
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
            ProductList = products.ToList()
        };

        return View(ProductUserVm);
    }

    // Итоговая страница корзины с товарами - post
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVm)
    {
        var PathTotemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                             "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

        var subject = "New Inquiry";
        string htmlBody = "";

        using (StreamReader streamReader = System.IO.File.OpenText(PathTotemplate))
        {
            htmlBody = streamReader.ReadToEnd();
        }
       //Name : {0}
       //Email  : {1}
       //Phone : {2}
       //Products: {3}

       StringBuilder productsListStringBuilder = new StringBuilder();

       foreach (var product in ProductUserVm.ProductList)
       {
           productsListStringBuilder.Append($" - Name{product.Name}<span style='font-size:14px;'>(- Id: {product.Id})</span><br />)");
       }

       string messageBody = string.Format(htmlBody,
           ProductUserVm.ApplicationUser.FullName,
           ProductUserVm.ApplicationUser.Email,
           ProductUserVm.ApplicationUser.PhoneNumber,
           productsListStringBuilder.ToString()
       );

       await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);
        return RedirectToAction(nameof(InquiryConfirmation));
    }

    public IActionResult InquiryConfirmation()
    {
        //очистить сессию
        HttpContext.Session.Clear();
        return View();
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