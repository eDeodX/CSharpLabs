using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Lab12.Pages;

public class PatientsModel(HospitalContext db) : PageModel
{
    public List<Patient> Rows { get; private set; } = new();

    [BindProperty]
    public Patient Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id.HasValue)
        {
            var row = await db.Patients.FindAsync(id.Value);
            if (row == null) return NotFound();
            Input = row;
        }
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (Input.BirthYear > DateTime.Today.Year)
            ModelState.AddModelError("Input.BirthYear", "Год рождения не может быть больше текущего.");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        Patient row;
        if (Input.Id == 0)
        {
            row = new Patient();
            db.Patients.Add(row);
        }
        else
        {
            var existing = await db.Patients.FindAsync(Input.Id);
            if (existing == null) return NotFound();
            row = existing;
        }

        row.Name = Input.Name.Trim();
        row.BirthYear = Input.BirthYear;
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await db.Patients.FindAsync(id);
        if (row == null) return NotFound();
        db.Patients.Remove(row);
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
        Rows = await db.Patients.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    }
}
