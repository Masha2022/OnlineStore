using Microsoft.AspNetCore.Mvc;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;

    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        IEnumerable<Category> objectList = _db.Category;
        return View(objectList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _db.Category.Add(category);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(category);
    }

    //get-edit
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        //Find работает только с полем, имеющим атрибут первичный ключ!
        var category = _db.Category.Find(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    //post - edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            _db.Category.Update(category);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(category);
    }

    //get-delete
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        var category = _db.Category.Find(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    //post - delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePost(int? id)
    {
        var category = _db.Category.Find(id);
        if (category == null)
        {
            return NotFound();
        }

        _db.Remove(category);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}