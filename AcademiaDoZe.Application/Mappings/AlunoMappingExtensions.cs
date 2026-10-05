using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class AlunoMappingExtensions
{
    public static AlunoDto ToDto(this Aluno entity)
    {
        if (entity == null) return null!;
        return new AlunoDto(
            entity.Id,
            entity.Nome,
            entity.Cpf.Valor,
            entity.DataNascimento,
            entity.Telefone.Valor,
            entity.Email.Valor,
            new LogradouroDto(entity.Endereco.LogradouroId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty),
            entity.Endereco.Numero,
            entity.Endereco.Complemento,
            entity.Foto != null ? new ArquivoDto(entity.Foto.Conteudo) : null
        );
    }

    public static Result<Aluno> ToEntity(this AlunoDto dto, string senha)
    {
        if (dto == null) return Result<Aluno>.Failure("Aluno", "ALUNO_INVALIDO");
        var logradouroResult = dto.Endereco.ToEntity();
        if (logradouroResult.IsFailure) return Result<Aluno>.Failure(logradouroResult.Notifications);

        if (dto.Foto == null)
            return Result<Aluno>.Failure("Foto", "ARQUIVO_OBRIGATORIO");

        var arquivoResult = Arquivo.Criar(dto.Foto.Conteudo);
        if (arquivoResult.IsFailure) return Result<Aluno>.Failure(arquivoResult.Notifications);

        return Aluno.Criar(dto.Id, dto.Nome, dto.Cpf, dto.DataNascimento, dto.Telefone, dto.Email, logradouroResult.Value!, dto.Numero, dto.Complemento ?? string.Empty, senha, arquivoResult.Value!);
    }
}
