namespace AcademiaDoZe.Application.DTOs;

public record PessoaDto
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
    ArquivoDto? Foto
);
