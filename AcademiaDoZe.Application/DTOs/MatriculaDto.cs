using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.DTOs;

public record MatriculaDto
(
    int Id,
    int AlunoId,
    MatriculaPlano Plano,
    DateOnly DataInicio,
    DateOnly DataFim,
    string Objetivo,
    MatriculaRestricoes RestricoesMedicas,
    string ObservacoesRestricoes,
    IReadOnlyList<MatriculaLaudoDto> LaudosMedicos
);
