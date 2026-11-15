using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lab12.Pages;

public class DoctorsModel(HospitalContext db) : PageModel
{
    public List<Doctor> Rows { get; private set; } = new();

    [BindProperty]
    public Doctor Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id.HasValue)
        {
            var row = await db.Doctors.FindAsync(id.Value);
            if (row == null) return NotFound();
            Input = row;
        }
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        Doctor row;
        if (Input.Id == 0)
        {
            row = new Doctor();
            db.Doctors.Add(row);
        }
        else
        {
            var existing = await db.Doctors.FindAsync(Input.Id);
            if (existing == null) return NotFound();
            row = existing;
        }

        row.Name = Input.Name.Trim();
        row.Specialty = Input.Specialty.Trim();
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await db.Doctors.FindAsync(id);
        if (row == null) return NotFound();
        db.Doctors.Remove(row);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is
            Microsoft.Data.Sqlite.SqliteException { SqliteExtendedErrorCode: 1811 or 787 })
        {
            Message = "Запись используется в обращении. Сначала удали или измени обращение.";
        }
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Rows = await db.Doctors.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    }
}
