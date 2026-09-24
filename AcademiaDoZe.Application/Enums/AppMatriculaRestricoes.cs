using System;
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

[Flags]
public enum AppMatriculaRestricoes
{
    [Display(Name = "Nenhuma")]
    None = 0,
    [Display(Name = "Cardíaca")]
    Cardiaca = 1,
    [Display(Name = "Respiratória")]
    Respiratoria = 2,
    [Display(Name = "Outras")]
    Outras = 4
}
