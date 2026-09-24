namespace AcademiaDoZe.Application.DTOs;

public record LogradouroDto
(
    int Id,
    string Cep,
    string Nome,
    string Bairro,
    string Cidade,
    string Estado,
    string Pais
);
