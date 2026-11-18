using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 2", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 2 ORDER BY data_hora", cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<AcessoColaborador>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
        return result;
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        var query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (2, @PessoaId, @DataHora)");
        await using var command = await CreateCommandAsync(query, cancellationToken);
        command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
        command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
        var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_COLABORADOR", "Falha ao obter ID do acesso do colaborador.", cancellationToken);
        var property = typeof(EntidadeBase).GetProperty(nameof(EntidadeBase.Id));
        property?.GetSetMethod(nonPublic: true)?.Invoke(entity, [id]);
        return entity;
    }

    public Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Acesso não pode ser alterado.");

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 2", cancellationToken);
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 2";
        if (colaboradorId.HasValue) sql += " AND pessoa_id = @ColaboradorId";
        if (inicio.HasValue) sql += " AND CAST(data_hora AS DATE) >= @Inicio";
        if (fim.HasValue) sql += " AND CAST(data_hora AS DATE) <= @Fim";
        sql += " ORDER BY data_hora";
        await using var command = await CreateCommandAsync(sql, cancellationToken);
        if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
        if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value, DbType.Date);
        if (fim.HasValue) command.AddParameter("@Fim", fim.Value, DbType.Date);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<AcessoColaborador>();
        while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
        return result;
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync("SELECT TOP 1 id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 2 AND pessoa_id = @ColaboradorId ORDER BY data_hora DESC, id_acesso DESC", cancellationToken);
        command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        var registros = (await ObterAcessosPorColaboradorPeriodo(colaboradorId, data, data, cancellationToken)).OrderBy(r => r.DataHora).ToList();
        var total = TimeSpan.Zero;
        for (var i = 0; i + 1 < registros.Count; i += 2)
            total += registros[i + 1].DataHora - registros[i].DataHora;
        if (registros.Count % 2 == 1)
            total += DateTime.Now - registros[^1].DataHora;
        return total;
    }

    private static AcessoColaborador Map(DbDataReader reader)
    {
        var id = reader.GetInt32(reader.GetOrdinal("id_acesso"));
        var pessoaId = reader.GetInt32(reader.GetOrdinal("pessoa_id"));
        var dataHora = reader.GetDateTime(reader.GetOrdinal("data_hora"));
        var result = AcessoColaborador.Criar(id, pessoaId, dataHora);
        if (result.IsFailure) throw new InfrastructureException("ERRO_MAPEAR_ACESSO_COLABORADOR", string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }
}
