using System.ComponentModel.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;
using OnlineStore.Models.ViewModels;

namespace OnlineStore.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        IEnumerable<Product> products = _db.Product.Include(p => p.Category).Include(p => p.ApplicationType);

       //foreach (var product in products)
       //{
       //    product.Category = _db.Category.FirstOrDefault(p => p.Id == product.CategoryId);
       //    product.ApplicationType = _db.ApplicationTypes.FirstOrDefault(p => p.Id == product.ApplicationTypeId);
       //}

        return View(products);
    }

    //get-upsert
    public IActionResult Upsert(int? id)
    {
        //IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(i => new SelectListItem
        //{
        //    Text = i.Name,
        //        Value = i.Id.ToString(),
        //});

        ////ViewBag.CategoryDropDown = CategoryDropDown;
        //ViewData["CategoryDropDown"] = CategoryDropDown;
        //
        //Product product = new Product();

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

    //post-upsert
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductVM productVm)
    {
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

    private static string CreateFile(string webRootPath, string fileName, IFormFileCollection files)
    {
        string upload = webRootPath + WC.ImagePath;
        string extension = Path.GetExtension(files[0].FileName);

        var oldFile = Path.Combine(upload, fileName);
        if (System.IO.File.Exists(oldFile))
        {
            System.IO.File.Delete(oldFile);
        }

        var productImagePath = fileName + extension;
        using (var fileStream = new FileStream(Path.Combine(upload, productImagePath), FileMode.Create))
        {
            files[0].CopyTo(fileStream);
        }


        return productImagePath;
    }

    //get-delete
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }

        var product = _db.Product.Include(p => p.Category).
            Include(p => p.ApplicationType).
            FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

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

        string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;

        var oldFile = Path.Combine(upload, product.Image);

        if (System.IO.File.Exists(oldFile))
        {
            System.IO.File.Delete(oldFile);
        }

        _db.Product.Remove(product);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}