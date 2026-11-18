using AcademiaDoZe.Domain.Common; /// Matheus Kautnick Domeneghini
namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : EntidadeBase, IAggregateRoot
{
    public int AlunoId { get; private set; }
    public DateTime DataHora { get; private set; }
    private AcessoAluno(int id, int alunoId, DateTime dataHora) : base(id)
    {
        AlunoId = alunoId;
        DataHora = dataHora;
    }
    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateTime dataHora)
        => Criar(id, aluno?.Id ?? 0, dataHora, aluno is null);

    public static Result<AcessoAluno> Criar(int id, int alunoId, DateTime dataHora)
        => Criar(id, alunoId, dataHora, alunoId <= 0);

    private static Result<AcessoAluno> Criar(int id, int alunoId, DateTime dataHora, bool alunoInvalido)
    {
        var notifications = new List<Notification>();
        if (alunoInvalido)
            notifications.Add(new Notification("Aluno", "ALUNO_INVALIDO"));
        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) || dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
            notifications.Add(new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);
        return Result<AcessoAluno>.Success(new AcessoAluno(id, alunoId, dataHora));
    }
}
// Dependem da persistência:
// Validar se possui matrícula ativa
// Na entrada, mostrar quanto tempo ainda tem de plano
// Na saída, mostrar o tempo que permaneceu na academia