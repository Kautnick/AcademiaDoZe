using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Domain.Enums;

[Flags]
public enum MatriculaRestricoes
{
    [Display(Name = "Nenhuma")]
    None = 0,
    [Display(Name = "Diabetes")]
    Diabetes = 1,
    [Display(Name = "Pressão alta")]
    PressaoAlta = 2,
    [Display(Name = "Labirintite")]
    Labirintite = 4,
    [Display(Name = "Alergias")]
    Alergias = 8,
    [Display(Name = "Problemas respiratórios")]
    ProblemasRespiratorios = 16,
    [Display(Name = "Uso contínuo de medicamentos")]
    RemedioContinuo = 32,
    [Display(Name = "Outras restrições")]
    Outras = 64
}
