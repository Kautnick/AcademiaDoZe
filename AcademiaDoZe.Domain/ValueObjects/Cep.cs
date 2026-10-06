using AcademiaDoZe.Domain.Common; /// Matheus Kautnick Domeneghini
using AcademiaDoZe.Domain.Services;
namespace AcademiaDoZe.Domain.ValueObjects;

public record Cep
{
    public string Valor { get; }
    private Cep(string valor)
    {
        Valor = valor;
    }
    public static Result<Cep> Criar(string valor)
    {
        if (NormalizacaoService.TextoVazioOuNulo(valor))
            return Result<Cep>.Failure("Cep", "CEP_OBRIGATORIO");

        var textoLimpo = valor.Trim();
        var formatoSemMascara = textoLimpo.Length == 8 && textoLimpo.All(EhDigitoAscii);
        var formatoComMascara = textoLimpo.Length == 9
            && textoLimpo[5] == '-'
            && textoLimpo.Where((_, indice) => indice != 5).All(EhDigitoAscii);

        if (!formatoSemMascara && !formatoComMascara)
            return Result<Cep>.Failure("Cep", "CEP_DIGITOS");

        return Result<Cep>.Success(new Cep(textoLimpo.Replace("-", string.Empty)));
    }

    private static bool EhDigitoAscii(char caractere) => caractere is >= '0' and <= '9';

    public override string ToString() => Valor;
}