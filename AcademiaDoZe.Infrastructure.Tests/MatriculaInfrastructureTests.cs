using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private const string NomeAluno = "Matheus Kautnick Domeneghini";
    private const string Sgbd = "SQLServer";

    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly MatriculaRepository _matriculaRepo;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        _matriculaRepo = new MatriculaRepository(ConnectionString, DatabaseType);
    }

    private async Task<Matricula> CriarEInserirMatriculaAsync(
        Aluno aluno,
        MatriculaPlano plano = MatriculaPlano.Mensal,
        DateOnly? dataInicio = null,
        MatriculaRestricoes restricoes = MatriculaRestricoes.None,
        string obsRestricao = Sgbd,
        Arquivo? laudo = null)
    {
        var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);
        if (restricoes != MatriculaRestricoes.None && laudo == null)
            laudo = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value;

        var result = Matricula.Criar(
            id: 0,
            aluno: aluno,
            plano: plano,
            dataInicio: inicio,
            objetivo: NomeAluno,
            restricoesMedicas: restricoes,
            laudoMedico: laudo,
            observacoesRestricoes: obsRestricao);

        if (result.IsFailure)
            throw new Exception($"Falha ao criar Matricula no Helper: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return await _matriculaRepo.Adicionar(result.Value!);
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var restricoes = MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta;
        var laudo = Arquivo.Criar(new byte[] { 100, 101, 102 }).Value;
        var inserida = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, restricoes: restricoes, obsRestricao: $"{Sgbd} - Usar medicação ao acordar", laudo: laudo);

        Assert.True(inserida.Id > 0);
        Assert.Equal(aluno.Id, inserida.AlunoId);
        Assert.Equal(NomeAluno, inserida.Objetivo);
        Assert.Contains(Sgbd, inserida.ObservacoesRestricoes);

        var obtida = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.NotNull(obtida);
        Assert.Equal(inserida.Id, obtida.Id);
        Assert.Equal(inserida.AlunoId, obtida.AlunoId);
        Assert.Equal(inserida.Plano, obtida.Plano);
        Assert.Equal(inserida.RestricoesMedicas, obtida.RestricoesMedicas);
        Assert.Equal(laudo!.Conteudo, obtida.LaudoMedico!.Conteudo);
    }

    [Fact]
    public async Task Matricula_RestricoesMedicas_PersisteEObtemCorretamente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var laudo = Arquivo.Criar(new byte[] { 10, 20, 30, 40, 50 }).Value!;
        var restricoes = MatriculaRestricoes.PressaoAlta | MatriculaRestricoes.Labirintite | MatriculaRestricoes.ProblemasRespiratorios | MatriculaRestricoes.RemedioContinuo;
        var matricula = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Semestral, restricoes: restricoes, obsRestricao: $"{Sgbd} - Evitar exercícios de alto impacto", laudo: laudo);
        var obtida = await _matriculaRepo.ObterPorId(matricula.Id);

        Assert.NotNull(obtida);
        Assert.Equal(restricoes, obtida.RestricoesMedicas);
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.PressaoAlta));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasRespiratorios));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.RemedioContinuo));
        Assert.Contains(Sgbd, obtida.ObservacoesRestricoes);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
        => Assert.Null(await _matriculaRepo.ObterPorId(999999));

    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno);
        var todas = await _matriculaRepo.ObterTodos();
        Assert.NotEmpty(todas);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, restricoes: MatriculaRestricoes.Alergias);
        var novasRestricoes = MatriculaRestricoes.Alergias | MatriculaRestricoes.Diabetes | MatriculaRestricoes.Labirintite;
        var laudo = Arquivo.Criar(new byte[] { 99, 88, 77 }).Value!;
        var atualizada = Matricula.Criar(inserida.Id, aluno, MatriculaPlano.Anual, inserida.DataInicio, NomeAluno, novasRestricoes, laudo, $"{Sgbd} - Restrição atualizada").Value!;

        var resultado = await _matriculaRepo.Atualizar(atualizada);
        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);
        Assert.Equal(MatriculaPlano.Anual, resultado.Plano);
        Assert.Equal(NomeAluno, resultado.Objetivo);
        Assert.NotNull(noBanco);
        Assert.Equal(MatriculaPlano.Anual, noBanco.Plano);
        Assert.Equal(NomeAluno, noBanco.Objetivo);
        Assert.Equal(novasRestricoes, noBanco.RestricoesMedicas);
    }

    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inexistente = Matricula.Criar(999999, aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), NomeAluno, MatriculaRestricoes.None, null, Sgbd).Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _matriculaRepo.Atualizar(inexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno);
        Assert.True(await _matriculaRepo.Remover(inserida.Id));
        Assert.Null(await _matriculaRepo.ObterPorId(inserida.Id));
    }

    [Fact]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
        => Assert.False(await _matriculaRepo.Remover(999999));

    [Fact]
    public async Task Matricula_ObterPorAluno_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno);
        var matriculas = await _matriculaRepo.ObterPorAluno(aluno.Id);
        Assert.NotEmpty(matriculas);
        Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoId));
    }

    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_E_PossuiMatriculaAtiva()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        Assert.False(await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id));
        await CriarEInserirMatriculaAsync(aluno);
        Assert.True(await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id));
        var ativa = await _matriculaRepo.ObterMatriculaAtivaPorAluno(aluno.Id);
        Assert.NotNull(ativa);
        Assert.Equal(aluno.Id, ativa.AlunoId);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Semestral);
        var gerais = await _matriculaRepo.ObterAtivas();
        var porAluno = await _matriculaRepo.ObterAtivas(aluno.Id);
        Assert.NotEmpty(gerais);
        Assert.NotEmpty(porAluno);
        Assert.All(porAluno, m => Assert.Equal(aluno.Id, m.AlunoId));
    }

    [Fact]
    public async Task Matricula_ObterVencendoEmDias_RetornaMatriculasProximasDoVencimento()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-25));
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, inicio);
        var vencendo = await _matriculaRepo.ObterVencendoEmDias(30);
        Assert.Contains(vencendo, m => m.AlunoId == aluno.Id);
    }

    [Fact]
    public async Task Matricula_ObterPorPlano_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Trimestral);
        var trimestrais = await _matriculaRepo.ObterPorPlano(MatriculaPlano.Trimestral);
        Assert.Contains(trimestrais, m => m.AlunoId == aluno.Id && m.Plano == MatriculaPlano.Trimestral);
    }
}
