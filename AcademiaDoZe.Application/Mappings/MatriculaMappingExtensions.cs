using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDto(this Matricula entity) => new(
        entity.Id,
        entity.AlunoId,
        entity.Plano,
        entity.DataInicio,
        entity.DataFim,
        entity.Objetivo,
        entity.RestricoesMedicas,
        entity.ObservacoesRestricoes,
        entity.LaudosMedicos.Select(laudo => new MatriculaLaudoDto(laudo.NomeArquivo, laudo.Conteudo.Conteudo)).ToArray());

    public static Result<Matricula> ToEntity(this MatriculaDto dto, Aluno aluno)
    {
        if (dto is null) return Result<Matricula>.Failure("Matricula", "MATRICULA_INVALIDA");
        if (aluno is null) return Result<Matricula>.Failure("Aluno", "ALUNO_INVALIDO");

        var laudos = new List<LaudoMatricula>();
        foreach (var dtoLaudo in dto.LaudosMedicos ?? [])
        {
            var laudo = LaudoMatricula.Criar(dtoLaudo.NomeArquivo, dtoLaudo.Conteudo);
            if (laudo.IsFailure) return Result<Matricula>.Failure(laudo.Notifications);
            laudos.Add(laudo.Value!);
        }

        return Matricula.CriarComLaudos(dto.Id, aluno, dto.Plano, dto.DataInicio, dto.Objetivo, dto.RestricoesMedicas, laudos, dto.ObservacoesRestricoes);
    }
}
