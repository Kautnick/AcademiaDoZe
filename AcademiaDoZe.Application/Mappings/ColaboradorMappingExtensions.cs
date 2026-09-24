using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDto(this Colaborador entity)
    {
        if (entity == null) return null!;
        return new ColaboradorDto(
            entity.Id,
            entity.Nome,
            entity.Cpf.Valor,
            entity.DataNascimento,
            entity.Telefone.Valor,
            entity.Email.Valor,
            entity.Endereco.Logradouro.ToDto(),
            entity.Endereco.Numero,
            entity.Endereco.Complemento,
            entity.Foto != null ? new ArquivoDto(entity.Foto.Conteudo) : null,
            entity.DataAdmissao,
            entity.Tipo,
            entity.Vinculo
        );
    }

    public static Result<Colaborador> ToEntity(this ColaboradorDto dto, string senha)
    {
        if (dto == null) return Result<Colaborador>.Failure("Colaborador", "COLABORADOR_INVALIDO");
        var logradouroResult = dto.Endereco.ToEntity();
        if (logradouroResult.IsFailure) return Result<Colaborador>.Failure(logradouroResult.Notifications);

        Arquivo? arquivo = null;
        if (dto.Foto != null)
        {
            var arquivoResult = Arquivo.Criar(dto.Foto.Conteudo);
            if (arquivoResult.IsFailure) return Result<Colaborador>.Failure(arquivoResult.Notifications);
            arquivo = arquivoResult.Value!;
        }

        return Colaborador.Criar(dto.Id, dto.Nome, dto.Cpf, dto.DataNascimento, dto.Telefone, dto.Email, logradouroResult.Value!, dto.Numero, dto.Complemento ?? string.Empty, senha, arquivo, dto.DataAdmissao, dto.Tipo, dto.Vinculo);
    }
}
