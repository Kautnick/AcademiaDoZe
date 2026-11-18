using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : EntidadeBase, IAggregateRoot
{
    public int ColaboradorId { get; private set; }
    public DateTime DataHora { get; private set; }
    private AcessoColaborador(int id, int colaboradorId, DateTime dataHora) : base(id)
    {
        ColaboradorId = colaboradorId;
        DataHora = dataHora;
    }
    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateTime dataHora)
        => Criar(id, colaborador?.Id ?? 0, dataHora, colaborador is null);

    public static Result<AcessoColaborador> Criar(int id, int colaboradorId, DateTime dataHora)
        => Criar(id, colaboradorId, dataHora, colaboradorId <= 0);

    private static Result<AcessoColaborador> Criar(int id, int colaboradorId, DateTime dataHora, bool colaboradorInvalido)
    {
        var notifications = new List<Notification>();
        if (colaboradorInvalido)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_INVALIDO"));
        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) || dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
            notifications.Add(new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);
        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaboradorId, dataHora));
    }
}
// Dependem da persistência:
// Validar se já não ultrapassa o limite de: 8 horas se for ctl, 6 horas se for estágio.
// Na saída, mostrar o tempo que permaneceu na academia, devendo ser somado todos os registros do dia.