using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro entity)
    {
        if (entity == null) return null!;
        return new LogradouroDto(entity.Id, entity.Cep.Valor, entity.Nome, entity.Bairro, entity.Cidade, entity.Estado, entity.Pais);
    }

    public static Result<Logradouro> ToEntity(this LogradouroDto dto)
    {
        if (dto == null) return Result<Logradouro>.Failure("Logradouro", "LOGRADOURO_INVALIDO");
        return Logradouro.Criar(dto.Id, dto.Cep, dto.Nome, dto.Bairro, dto.Cidade, dto.Estado, dto.Pais);
    }
}
