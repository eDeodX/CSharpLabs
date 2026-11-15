using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Lab12.Pages;

public class AdmissionsModel(HospitalContext db) : PageModel
{
    public List<Admission> Rows { get; private set; } = new();

    [BindProperty]
    public Admission Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public SelectList PatientOptions { get; private set; } = null!;
    public SelectList DoctorOptions { get; private set; } = null!;
    public bool CanAdd { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Input.Date = DateOnly.FromDateTime(DateTime.Today);
        if (id.HasValue)
        {
            var row = await db.Admissions.FindAsync(id.Value);
            if (row == null) return NotFound();
            Input = row;
        }
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (Input.Date == default)
            ModelState.AddModelError("Input.Date", "Укажи дату обращения.");
        if (!await db.Patients.AnyAsync(p => p.Id == Input.PatientId))
            ModelState.AddModelError("Input.PatientId", "Пациент не найден.");
        if (!await db.Doctors.AnyAsync(d => d.Id == Input.DoctorId))
            ModelState.AddModelError("Input.DoctorId", "Врач не найден.");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        Admission row;
        if (Input.Id == 0)
        {
            row = new Admission();
            db.Admissions.Add(row);
        }
        else
        {
            var existing = await db.Admissions.FindAsync(Input.Id);
            if (existing == null) return NotFound();
            row = existing;
        }

        row.PatientId = Input.PatientId;
        row.DoctorId = Input.DoctorId;
        row.Date = Input.Date;
        row.Complaint = Input.Complaint.Trim();
        await db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await db.Admissions.FindAsync(id);
        if (row == null) return NotFound();
        db.Admissions.Remove(row);
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
        Rows = await db.Admissions.AsNoTracking()
            .Include(a => a.Patient).Include(a => a.Doctor)
            .OrderByDescending(a => a.Date).ThenByDescending(a => a.Id).ToListAsync();
        var patients = await db.Patients.OrderBy(p => p.Name).ToListAsync();
        var doctors = await db.Doctors.OrderBy(d => d.Name).ToListAsync();
        PatientOptions = new SelectList(patients, "Id", "Name");
        DoctorOptions = new SelectList(doctors, "Id", "Name");
        CanAdd = patients.Count > 0 && doctors.Count > 0;
    }
}
