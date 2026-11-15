using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// Поле Picture исходной таблицы допускает NULL и не требуется для CRUD.
public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
}

public class CategoryInput
{
    [Required]
    [StringLength(15)]
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
}

public class NorthwindContext(DbContextOptions<NorthwindContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var category = model.Entity<Category>();
        category.ToTable("Categories");
        category.Property(c => c.CategoryId).HasColumnName("CategoryID");
        category.Property(c => c.CategoryName).HasMaxLength(15).IsRequired();
        if (Database.IsSqlServer())
            category.Property(c => c.Description).HasColumnType("ntext");
    }
}
