using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.DTOs;

public record MatriculaDto
(
    int Id,
    int AlunoId,
    AppMatriculaPlano Plano,
    System.DateOnly DataInicio,
    System.DateOnly DataFim,
    string Objetivo,
    AppMatriculaRestricoes RestricoesMedicas,
    string ObservacoesRestricoes,
    ArquivoDto? LaudoMedico
);
