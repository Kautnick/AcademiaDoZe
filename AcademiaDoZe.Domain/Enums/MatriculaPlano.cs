using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Domain.Enums;

public enum MatriculaPlano
{
    [Display(Name = "Mensal")]
    Mensal = 1,
    [Display(Name = "Trimestral")]
    Trimestral = 2,
    [Display(Name = "Semestral")]
    Semestral = 3,
    [Display(Name = "Anual")]
    Anual = 4
}
