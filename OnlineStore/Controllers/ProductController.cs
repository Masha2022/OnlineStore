using System.ComponentModel.Design;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;
[Authorize(Roles = WC.AdminRole)]// Атрибут, который требует наличие роли "AdminRole" для доступа к методам этого контроллера.
public class ProductController : Controller
{
    // Зависимости: контекст базы данных и среда веб-хостинга
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Конструктор контроллера, который принимает зависимости.
    public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db; // Инициализация контекста базы данных.
        _webHostEnvironment = webHostEnvironment; // Инициализация среды веб-хостинга.
    }

    // Метод для отображения всех продуктов.
    public IActionResult Index()
    {
        // Получение всех продуктов с их категориями и типами приложений.
        IEnumerable<Product> products = _db.Product.Include(p => p.Category).Include(p => p.ApplicationType);
        return View(products);
    }

    // Метод для добавления или редактирования продукта.
    public IActionResult Upsert(int? id)
    {
        // Создание представления модели с выпадающими списками категорий и типов приложений.
        ProductVM productVM = new ProductVM()
        {
            Product = new Product(),
            CategorySelectList = _db.Category.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString(),
            }),
            ApplicationTypeSelectList = _db.ApplicationTypes.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString(),
            })
        };

        // Если id равно null, это добавление нового продукта, иначе - редактирование существующего.
        if (id == null)
        {
            return View(productVM);
        }
        else
        {
            productVM.Product = _db.Product.Find(id);
            if (productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM);
        }
    }

    // Метод для добавления или редактирования продукта.
    //post-upsert
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductVM productVm)
    {
        // Проверка корректности данных.
        if (ModelState.IsValid)
        {
            var files = HttpContext.Request.Form.Files;
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (productVm.Product.Id == 0)
            {
                //creating
                string fileName = Guid.NewGuid().ToString();
                productVm.Product.Image = CreateFile(webRootPath, fileName, files);
                _db.Product.Add(productVm.Product);
            }
            else
            {
                //updating
                var objectFromDb = _db.Product.AsNoTracking().FirstOrDefault(p => p.Id == productVm.Product.Id);
                productVm.Product.Image = files.Count > 0
                    ? CreateFile(webRootPath, objectFromDb.Image, files)
                    : objectFromDb.Image;
                _db.Product.Update(productVm.Product);
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        // Если данные не прошли проверку, то подготовка данных для повторного отображения формы.
        productVm.CategorySelectList = _db.Category.Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Id.ToString(),
        });
        productVm.ApplicationTypeSelectList = _db.ApplicationTypes.Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Id.ToString(),
        });

        if (!ModelState.IsValid)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"{state.Key} : {error.ErrorMessage}");
                }
            }
        }

        return View(productVm);
    }
// Метод для создания файла на сервере.
    private static string CreateFile(string webRootPath, string fileName, IFormFileCollection files)
    {
        // Определение пути для загрузки изображений.
        string upload = webRootPath + WC.ImagePath;
        // Получение расширения из первого файла в коллекции.
        string extension = Path.GetExtension(files[0].FileName);

        // Создание полного пути к старому файлу (если он существует).
        var oldFile = Path.Combine(upload, fileName);
        // Проверка существования файла и его удаление, если он существует.
        if (System.IO.File.Exists(oldFile))
        {
            System.IO.File.Delete(oldFile);
        }

        // Создание полного пути к новому файлу изображения.
        var productImagePath = fileName + extension;
        // Создание нового файла изображения на сервере.
        using (var fileStream = new FileStream(Path.Combine(upload, productImagePath), FileMode.Create))
        {
            files[0].CopyTo(fileStream);
        }

        // Возвращение пути к созданному файлу изображения.
        return productImagePath;
    }

    // Метод для отображения формы удаления продукта.
    //get-delete
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        // Получение продукта по id.
        var product = _db.Product.Include(p => p.Category).
            Include(p => p.ApplicationType).
            FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
    
    // Метод для выполнения удаления продукта.
    //post - delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var product = _db.Product.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        // Удаление изображения продукта.
        string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;

        var oldFile = Path.Combine(upload, product.Image);

        if (System.IO.File.Exists(oldFile))
        {
            System.IO.File.Delete(oldFile);
        }

        // Удаление продукта из базы данных.
        _db.Product.Remove(product);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}