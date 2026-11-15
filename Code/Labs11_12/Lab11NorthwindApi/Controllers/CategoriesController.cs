using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/categories")]
public class CategoriesController(NorthwindContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<Category>> List()
    {
        return await db.Categories.AsNoTracking().OrderBy(c => c.CategoryId).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> Get(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return category;
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create(CategoryInput input)
    {
        var category = new Category
        {
            CategoryName = input.CategoryName.Trim(),
            Description = input.Description
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = category.CategoryId }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryInput input)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        category.CategoryName = input.CategoryName.Trim();
        category.Description = input.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null) return NotFound();
        db.Categories.Remove(category);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 547 })
        {
            return Conflict("Категория используется товарами.");
        }
        return NoContent();
    }
}
