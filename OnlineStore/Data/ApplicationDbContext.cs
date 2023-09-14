using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;

namespace OnlineStore.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Category> Category { get; set; }
    public DbSet<ApplicationType> ApplicationTypes { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<ApplicationUser> ApplicationUser { get; set; }
}