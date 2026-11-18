using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 1 ORDER BY data_hora", cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<AcessoAluno>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
        return result;
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (1, @PessoaId, @DataHora)");
        await using var command = await CreateCommandAsync(query, cancellationToken);
        command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter ID do acesso do aluno.", cancellationToken);
        var property = typeof(EntidadeBase).GetProperty(nameof(EntidadeBase.Id));
        property?.GetSetMethod(nonPublic: true)?.Invoke(entity, [id]);
        return entity;
    }

    public Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Acesso não pode ser alterado.");

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 1";
        if (alunoId.HasValue) sql += " AND pessoa_id = @AlunoId";
        if (inicio.HasValue) sql += " AND CAST(data_hora AS DATE) >= @Inicio";
        if (fim.HasValue) sql += " AND CAST(data_hora AS DATE) <= @Fim";
        sql += " ORDER BY data_hora";
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
        if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value, DbType.Date);
        if (fim.HasValue) command.AddParameter("@Fim", fim.Value, DbType.Date);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<AcessoAluno>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
        return result;
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT TOP 1 id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 1 AND pessoa_id = @AlunoId ORDER BY data_hora DESC, id_acesso DESC", cancellationToken);
        command.AddParameter("@AlunoId", alunoId, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT COUNT(*) FROM tb_acesso WHERE pessoa_tipo = 1 AND pessoa_id = @AlunoId AND CAST(data_hora AS DATE) = CAST(GETDATE() AS DATE)", cancellationToken);
        command.AddParameter("@AlunoId", alunoId, DbType.Int32);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        return count % 2 == 1;
    }

    public Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
        => Task.FromResult(new Dictionary<TimeOnly, int>());

    public Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
        => Task.FromResult(new Dictionary<int, TimeSpan>());

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
        => await Task.FromResult(Array.Empty<Aluno>());

    private static AcessoAluno Map(DbDataReader reader)
    {
        var id = reader.GetInt32(reader.GetOrdinal("id_acesso"));
        var pessoaId = reader.GetInt32(reader.GetOrdinal("pessoa_id"));
        var dataHora = reader.GetDateTime(reader.GetOrdinal("data_hora"));
        var result = AcessoAluno.Criar(id, pessoaId, dataHora);
        if (result.IsFailure) throw new InfrastructureException("ERRO_MAPEAR_ACESSO_ALUNO", string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }
}
