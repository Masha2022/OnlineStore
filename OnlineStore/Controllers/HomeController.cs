using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;
using OnlineStore.Utility;

namespace OnlineStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _dbContext;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public IActionResult Index()
    {
        HomeVM homeVm = new HomeVM()
        {
            Products = _dbContext.Product.Include(p => p.Category).Include(p => p.ApplicationType),
            Categories = _dbContext.Category
        };
        return View(homeVm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Details(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
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
                .FirstOrDefault(),
            ExistInCart = false
        };

        foreach (var item in shoppingCartList)
        {
            if (item.ProductId == id)
            {
                detailsVm.ExistInCart = true;
            }
        }
        return View(detailsVm);
    }
    
    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        shoppingCartList.Add(new ShoppingCart { ProductId = id });
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult RemoveFromCart(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
        if (itemToRemove != null)
        {
            shoppingCartList.Remove(itemToRemove);
        }
        
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }
}