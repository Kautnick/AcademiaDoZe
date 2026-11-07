using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public class Matricula : EntidadeBase, IAggregateRoot
{
    public int AlunoId { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes RestricoesMedicas { get; private set; }
    public string ObservacoesRestricoes { get; private set; }
    public IReadOnlyList<LaudoMatricula> LaudosMedicos { get; private set; }
    public Arquivo? LaudoMedico => LaudosMedicos.FirstOrDefault()?.Conteudo;

    private Matricula(int id, int alunoId, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim, string objetivo, MatriculaRestricoes restricoesMedicas, IReadOnlyList<LaudoMatricula> laudosMedicos, string observacoesRestricoes = "") : base(id)
    {
        AlunoId = alunoId;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Objetivo = objetivo;
        RestricoesMedicas = restricoesMedicas;
        LaudosMedicos = laudosMedicos.ToArray();
        ObservacoesRestricoes = observacoesRestricoes;
    }

    public static Result<Matricula> Criar(int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico, string observacoesRestricoes = "")
    {
        var laudos = laudoMedico is null
            ? Array.Empty<LaudoMatricula>()
            : new[] { LaudoMatricula.Criar("laudo-medico", laudoMedico.Conteudo).Value! };
        return CriarComLaudos(id, aluno, plano, dataInicio, objetivo, restricoesMedicas, laudos, observacoesRestricoes);
    }

    public static Result<Matricula> CriarComLaudos(int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, string objetivo, MatriculaRestricoes restricoesMedicas, IReadOnlyCollection<LaudoMatricula> laudosMedicos, string observacoesRestricoes = "")
    {
        var notifications = new List<Notification>();
        laudosMedicos ??= Array.Empty<LaudoMatricula>();
        if (aluno == null)
        {
            notifications.Add(new Notification("Aluno", "ALUNO_INVALIDO"));
        }
        else if (dataInicio != default && IdadeNaData(aluno.DataNascimento, dataInicio) is >= 12 and <= 16 && laudosMedicos.Count == 0)
        {
            notifications.Add(new Notification("LaudosMedicos", "LAUDO_12_A_16_OBRIGATORIO"));
        }

        if (!Enum.IsDefined(plano)) notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));
        if (dataInicio == default) notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIO"));
        const MatriculaRestricoes restricoesValidas = MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta | MatriculaRestricoes.Labirintite | MatriculaRestricoes.Alergias | MatriculaRestricoes.ProblemasRespiratorios | MatriculaRestricoes.RemedioContinuo | MatriculaRestricoes.Outras;
        if ((restricoesMedicas & ~restricoesValidas) != 0) notifications.Add(new Notification("RestricoesMedicas", "RESTRICOES_INVALIDAS"));

        DateOnly dataFim = default;
        if (Enum.IsDefined(plano) && dataInicio != default)
        {
            dataFim = plano switch
            {
                MatriculaPlano.Mensal => dataInicio.AddMonths(1),
                MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
                MatriculaPlano.Semestral => dataInicio.AddMonths(6),
                MatriculaPlano.Anual => dataInicio.AddYears(1),
                _ => default
            };
        }

        if (NormalizacaoService.TextoVazioOuNulo(objetivo)) notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else
        {
            objetivo = NormalizacaoService.LimparEspacos(objetivo);
            if (objetivo.Length > 500) notifications.Add(new Notification("Objetivo", "OBJETIVO_TAMANHO_MAXIMO"));
        }
        if (restricoesMedicas != MatriculaRestricoes.None && laudosMedicos.Count == 0) notifications.Add(new Notification("LaudosMedicos", "RESTRICOES_LAUDO_OBRIGATORIO"));
        if (laudosMedicos.Any(laudo => laudo is null)) notifications.Add(new Notification("LaudosMedicos", "LAUDO_INVALIDO"));
        observacoesRestricoes = NormalizacaoService.LimparEspacos(observacoesRestricoes);
        if (observacoesRestricoes.Length > 500) notifications.Add(new Notification("ObservacoesRestricoes", "OBSERVACOES_TAMANHO_MAXIMO"));
        if (notifications.Count != 0) return Result<Matricula>.Failure(notifications);

        var matricula = new Matricula(id, aluno!.Id, plano, dataInicio, dataFim, objetivo, restricoesMedicas, laudosMedicos.ToArray(), observacoesRestricoes);
        return Result<Matricula>.Success(matricula);
    }

    public Matricula ComLaudosMedicos(IEnumerable<LaudoMatricula> laudosMedicos)
    {
        ArgumentNullException.ThrowIfNull(laudosMedicos);
        return new Matricula(Id, AlunoId, Plano, DataInicio, DataFim, Objetivo, RestricoesMedicas, laudosMedicos.ToArray(), ObservacoesRestricoes);
    }

    private static int IdadeNaData(DateOnly nascimento, DateOnly data)
    {
        var idade = data.Year - nascimento.Year;
        if (nascimento.AddYears(idade) > data) idade--;
        return idade;
    }
}
