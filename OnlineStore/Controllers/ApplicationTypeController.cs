using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Controllers;

// Контроллер для работы с типами приложений.
[Authorize(Roles = WC.AdminRole)] // Доступ разрешен только для роли администратора.
public class ApplicationTypeController : Controller
{
    private readonly ApplicationDbContext _db;

    // Конструктор: инжекция зависимости для контекста базы данных.
    public ApplicationTypeController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Метод для отображения списка типов приложений.
    public IActionResult Index()
    {
        IEnumerable<ApplicationType> objectList = _db.ApplicationTypes;
        return View(objectList);
    }

    // Метод для отображения страницы создания нового типа приложения.
    public IActionResult Create()
    {
        return View();
    }

    // Метод для обработки POST-запроса на создание нового типа приложения.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ApplicationType type)
    {
        _db.ApplicationTypes.Add(type);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    // Метод для отображения страницы редактирования существующего типа приложения.
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        //Find работает только с полем, имеющим атрибут первичный ключ!
        var model = _db.ApplicationTypes.Find(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // Метод для обработки POST-запроса на редактирование типа приложения.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ApplicationType type)
    {
        if (ModelState.IsValid)
        {
            _db.ApplicationTypes.Update(type);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(type);
    }

    // Метод для отображения страницы удаления типа приложения.
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        var type = _db.ApplicationTypes.Find(id);
        if (type == null)
        {
            return NotFound();
        }

        return View(type);
    }

    // Метод для обработки POST-запроса на удаление типа приложения.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var entity = _db.ApplicationTypes.Find(id);
        if (entity == null)
        {
            return NotFound();
        }

        _db.Remove(entity);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}