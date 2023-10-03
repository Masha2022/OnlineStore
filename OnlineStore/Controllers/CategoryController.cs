using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Controllers;
// Контроллер для работы с категориями продукции в интернет-магазине.
[Authorize(Roles = WC.AdminRole)] // Доступ к этому контроллеру разрешен только пользователям с ролью администратора
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db; // Ссылка на контекст базы данных для доступа к данным

    // Конструктор контроллера с зависимостью от контекста базы данных
    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Отображение списка категорий
    public IActionResult Index()
    {
        IEnumerable<Category> objectList = _db.Category;
        return View(objectList);
    }

    // Отображение страницы создания новой категории
    public IActionResult Create()
    {
        return View();
    }

    // Обработка POST-запроса для создания новой категории
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid) // Проверка на валидность введенных данных
        {
            _db.Category.Add(category); // Добавление новой категории
            _db.SaveChanges(); // Сохранение изменений в базе данных
            return RedirectToAction("Index"); // Перенаправление на список категорий
        }

        return View(category); // Возвращение на страницу создания с введенными данными
    }

    // Отображение страницы редактирования категории
    public IActionResult Edit(int? id) // id категории передается как необязательный параметр
    {
        if (id == null || id == 0) // Проверка на валидность ID
        {
            return NotFound();
        }

        var category = _db.Category.Find(id); // Поиск категории по ID

        if (category == null) // Если категория не найдена
        {
            return NotFound();
        }

        return View(category);
    }

    // Обработка POST-запроса для редактирования категории
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        if (ModelState.IsValid) // Проверка на валидность введенных данных
        {
            _db.Category.Update(category); // Обновление данных категории
            _db.SaveChanges(); // Сохранение изменений в базе данных
            return RedirectToAction("Index"); // Перенаправление на список категорий
        }

        return View(category); // Возвращение на страницу редактирования с введенными данными
    }

    // Отображение страницы удаления категории
    public IActionResult Delete(int? id) // id категории передается как необязательный параметр
    {
        if (id == null || id == 0) // Проверка на валидность ID
        {
            return NotFound();
        }

        var category = _db.Category.Find(id); // Поиск категории по ID

        if (category == null) // Если категория не найдена
        {
            return NotFound();
        }

        return View(category);
    }

    // Обработка POST-запроса для удаления категории
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var category = _db.Category.Find(id); // Поиск категории по ID
        if (category == null) // Если категория не найдена
        {
            return NotFound();
        }

        _db.Remove(category); // Удаление категории
        _db.SaveChanges(); // Сохранение изменений в базе данных
        return RedirectToAction("Index"); // Перенаправление на список категорий
    }
}