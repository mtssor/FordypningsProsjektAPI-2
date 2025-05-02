using Emne9_Fordypningsprosjekt_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Emne9_Fordypningsprosjekt_API.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }
}