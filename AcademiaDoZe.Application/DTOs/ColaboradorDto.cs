using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.DTOs;

public record ColaboradorDto
(
    int Id,
    string Nome,
    string Cpf,
    System.DateOnly DataNascimento,
    string Telefone,
    string Email,
    LogradouroDto Endereco,
    string Numero,
    string? Complemento,
    ArquivoDto? Foto,
    System.DateOnly DataAdmissao,
    ColaboradorTipo Tipo,
    ColaboradorVinculo Vinculo
);
