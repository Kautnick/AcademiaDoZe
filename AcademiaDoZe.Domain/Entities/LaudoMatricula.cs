using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public sealed record LaudoMatricula
{
    public string NomeArquivo { get; }
    public Arquivo Conteudo { get; }

    private LaudoMatricula(string nomeArquivo, Arquivo conteudo)
    {
        NomeArquivo = nomeArquivo;
        Conteudo = conteudo;
    }

    public static Result<LaudoMatricula> Criar(string nomeArquivo, byte[] conteudo)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            return Result<LaudoMatricula>.Failure("NomeArquivo", "NOME_ARQUIVO_LAUDO_OBRIGATORIO");

        var arquivo = Arquivo.Criar(conteudo);
        if (arquivo.IsFailure)
            return Result<LaudoMatricula>.Failure(arquivo.Notifications);

        return Result<LaudoMatricula>.Success(new LaudoMatricula(Path.GetFileName(nomeArquivo.Trim()), arquivo.Value!));
    }
}
