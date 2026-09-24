using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDto(this Matricula entity)
    {
        if (entity == null) return null!;
        return new MatriculaDto(
            entity.Id,
            entity.AlunoId,
            MapPlano(entity.Plano),
            entity.DataInicio,
            entity.DataFim,
            entity.Objetivo,
            MapRestricoes(entity.RestricoesMedicas),
            entity.ObservacoesRestricoes,
            entity.LaudoMedico != null ? new ArquivoDto(entity.LaudoMedico.Conteudo) : null
        );
    }

    private static AppMatriculaPlano MapPlano(MatriculaPlano plano) => plano switch
    {
        MatriculaPlano.Mensal => AppMatriculaPlano.Mensal,
        MatriculaPlano.Trimestral => AppMatriculaPlano.Trimestral,
        MatriculaPlano.Semestral => AppMatriculaPlano.Semestral,
        MatriculaPlano.Anual => AppMatriculaPlano.Anual,
        _ => AppMatriculaPlano.Mensal
    };

    private static AppMatriculaRestricoes MapRestricoes(MatriculaRestricoes restricoes)
    {
        AppMatriculaRestricoes result = AppMatriculaRestricoes.None;
        if (restricoes == MatriculaRestricoes.None) return result;
        if (restricoes.HasFlag(MatriculaRestricoes.Diabetes)) result |= AppMatriculaRestricoes.Outras;
        if (restricoes.HasFlag(MatriculaRestricoes.ProblemasRespiratorios)) result |= AppMatriculaRestricoes.Respiratoria;
        if (restricoes.HasFlag(MatriculaRestricoes.PressaoAlta)) result |= AppMatriculaRestricoes.Outras;
        if (restricoes.HasFlag(MatriculaRestricoes.Labirintite)) result |= AppMatriculaRestricoes.Outras;
        if (restricoes.HasFlag(MatriculaRestricoes.Alergias)) result |= AppMatriculaRestricoes.Outras;
        if (restricoes.HasFlag(MatriculaRestricoes.RemedioContinuo)) result |= AppMatriculaRestricoes.Outras;
        return result;
    }

    public static Result<Matricula> ToEntity(this MatriculaDto dto, Aluno aluno)
    {
        if (dto == null) return Result<Matricula>.Failure("Matricula", "MATRICULA_INVALIDA");
        if (aluno == null) return Result<Matricula>.Failure("Aluno", "ALUNO_INVALIDO");

        // Map plano
        var plano = dto.Plano switch
        {
            AppMatriculaPlano.Mensal => MatriculaPlano.Mensal,
            AppMatriculaPlano.Trimestral => MatriculaPlano.Trimestral,
            AppMatriculaPlano.Semestral => MatriculaPlano.Semestral,
            AppMatriculaPlano.Anual => MatriculaPlano.Anual,
            _ => MatriculaPlano.Mensal
        };

        // Map restricoes
        MatriculaRestricoes restricoes = MatriculaRestricoes.None;
        if (dto.RestricoesMedicas.HasFlag(AppMatriculaRestricoes.Respiratoria)) restricoes |= MatriculaRestricoes.ProblemasRespiratorios;
        if (dto.RestricoesMedicas.HasFlag(AppMatriculaRestricoes.Outras)) restricoes |= MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta | MatriculaRestricoes.Labirintite | MatriculaRestricoes.Alergias | MatriculaRestricoes.RemedioContinuo;

        Arquivo? laudo = null;
        if (dto.LaudoMedico != null)
        {
            var arquivoResult = Arquivo.Criar(dto.LaudoMedico.Conteudo);
            if (arquivoResult.IsFailure) return Result<Matricula>.Failure(arquivoResult.Notifications);
            laudo = arquivoResult.Value!;
        }

        return Matricula.Criar(dto.Id, aluno, plano, dto.DataInicio, dto.Objetivo, restricoes, laudo, dto.ObservacoesRestricoes ?? string.Empty);
    }
}
