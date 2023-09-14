using Microsoft.AspNetCore.Mvc;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Controllers;

public class ApplicationTypeController : Controller
{
    private readonly ApplicationDbContext _db;
    
    public ApplicationTypeController(ApplicationDbContext db)
    {
        _db = db;
    }
    public IActionResult Index()
    {
        IEnumerable<ApplicationType> objectList = _db.ApplicationTypes;
        return View(objectList);
    }
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ApplicationType type)
    {
        _db.ApplicationTypes.Add(type);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
    
    //get-edit
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

    //post - edit
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

    //get-delete
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

    //post - delete
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