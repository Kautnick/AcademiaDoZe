using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

public enum AppColaboradorTipo
{
    [Display(Name = "Instrutor")]
    Instrutor = 0,
    [Display(Name = "Administrador")]
    Administrador = 1
}
