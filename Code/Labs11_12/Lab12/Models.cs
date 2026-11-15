using System.ComponentModel.DataAnnotations;

namespace Lab12;

public class Patient
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "ФИО пациента")]
    public string Name { get; set; } = "";

    [Range(1900, 2100)]
    [Display(Name = "Год рождения")]
    public int BirthYear { get; set; }
}

public class Doctor
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "ФИО врача")]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(80)]
    [Display(Name = "Специальность")]
    public string Specialty { get; set; } = "";
}

public class Admission
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выбери пациента.")]
    [Display(Name = "Пациент")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выбери врача.")]
    [Display(Name = "Врач")]
    public int DoctorId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата обращения")]
    public DateOnly Date { get; set; }

    [Required]
    [StringLength(300)]
    [Display(Name = "Жалоба")]
    public string Complaint { get; set; } = "";

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}
