using AcademiaDoZe.Domain.Entities; // Matheus Kautnick Domeneghini
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;
    private static Aluno GetValidAluno(DateOnly? dataNascimento = null)
    {
        var nascimento = dataNascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20));
        return Aluno.Criar(1, "João da Silva", "529.982.247-25", nascimento, "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", string.Empty, "Abcdef", GetValidArquivo()).Value!;
    }

    [Theory(DisplayName = "Matricula: plano inválido -> PLANO_INVALIDO; válido -> sucesso")]
    [InlineData(999)]
    [InlineData((int)MatriculaPlano.Mensal)]
    public void Deve_FalharOuPassar_Criacao_Quando_ValorDoPlano(int planoValue)
    {
        var aluno = GetValidAluno();
        var plano = (MatriculaPlano)planoValue;
        var result = Matricula.Criar(1, aluno, plano, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, null);
        if (planoValue == 999)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "PLANO_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(aluno.Id, result.Value!.AlunoId);
        }
    }

    [Theory(DisplayName = "Matricula: data inicio obrigatória -> DATA_INICIO_OBRIGATORIO")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_DataInicioPadrao(bool useDefault)
    {
        var aluno = GetValidAluno();
        var inicio = useDefault ? default(DateOnly) : DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(1, aluno, MatriculaPlano.Mensal, inicio, "Objetivo", MatriculaRestricoes.None, null);
        if (useDefault)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_INICIO_OBRIGATORIO");
        }
        else
        {
            Assert.True(result.IsSuccess);
        }
    }

    [Theory(DisplayName = "Matricula: calcular DataFim por plano (mensal/trimestral/semestral/anual)")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public void Deve_Calcular_DataFim_Corretamente(MatriculaPlano plano, int meses)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(1, aluno, plano, inicio, "Objetivo", MatriculaRestricoes.None, null);
        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
    }

    [Theory(DisplayName = "Matricula: restrições -> quando presentes, laudo e observações devem existir")]
    [InlineData(MatriculaRestricoes.None, true)]
    [InlineData(MatriculaRestricoes.Diabetes, true)]
    [InlineData(MatriculaRestricoes.Diabetes | MatriculaRestricoes.Alergias, true)]
    public void Deve_Tratar_Restricoes_ComOuSemLaudo(MatriculaRestricoes restricoes, bool expectSuccess)
    {
        var aluno = GetValidAluno();
        Arquivo? laudo = null;
        string? observacoes = null;
        if (restricoes != MatriculaRestricoes.None)
        {
            laudo = GetValidArquivo();
            observacoes = "Observacoes";
        }
        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Objetivo",
            restricoes,
            laudo,
            observacoes
        );
        Assert.Equal(expectSuccess, result.IsSuccess);
    }

    [Theory(DisplayName = "Matrícula: alunos de 12 a 16 anos exigem laudo médico")]
    [InlineData(12, true)]
    [InlineData(16, true)]
    [InlineData(17, false)]
    public void Deve_Exigir_Laudo_Para_Idades_De_12_A_16(int age, bool expectFailure)
    {
        var aluno = GetValidAluno(DateOnly.FromDateTime(DateTime.Today.AddYears(-age)));
        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Melhorar condicionamento",
            MatriculaRestricoes.None,
            null
        );
        Assert.Equal(expectFailure, result.IsFailure);
        if (expectFailure)
            Assert.Contains(result.Notifications, n => n.Mensagem == "LAUDO_12_A_16_OBRIGATORIO");
    }

    [Theory(DisplayName = "Matricula: criação e cálculo de DataFim para planos comuns")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    public void Deve_Criar_Com_Sucesso_E_Calcular_DataFim(MatriculaPlano plano, int meses)
    {
        var aluno = GetValidAluno();
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(
            1,
            aluno,
            plano,
            inicio,
            "Melhorar condicionamento",
            MatriculaRestricoes.None,
            null
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
    }

    [Theory(DisplayName = "Matricula: múltiplas restrições são aceitas como flags e exigem laudo")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_RestricoesSemLaudo(bool provideLaudo)
    {
        var aluno = GetValidAluno();
        Arquivo? laudo = provideLaudo ? GetValidArquivo() : null;
        var observacoes = provideLaudo ? "obs" : "";
        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Objetivo",
            MatriculaRestricoes.Diabetes,
            laudo,
            observacoes
        );
        if (provideLaudo)
        {
            Assert.True(result.IsSuccess);
        }
        else
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "RESTRICOES_LAUDO_OBRIGATORIO");
        }
    }

    [Theory(DisplayName = "Matricula: normaliza observações de restrições removendo espaços extras")]
    [InlineData(" observa testo ", "observa testo")]
    [InlineData(" obs outro ", "obs outro")]
    public void Deve_Normalizar_ObservacoesRestricoes_Quando_InputTemEspacosExtras(string input, string expected)
    {
        var aluno = GetValidAluno();
        var result = Matricula.Criar(
            1,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Objetivo",
            MatriculaRestricoes.Diabetes,
            GetValidArquivo(),
            input
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.ObservacoesRestricoes);
    }

    [Fact]
    public void Deve_Aceitar_VariosLaudos_EmUmaMatricula()
    {
        var aluno = GetValidAluno();
        var laudos = new[]
        {
            LaudoMatricula.Criar("parecer.pdf", new byte[] { 1, 2 }).Value!,
            LaudoMatricula.Criar("exame.png", new byte[] { 3, 4 }).Value!
        };

        var resultado = Matricula.CriarComLaudos(
            1, aluno, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today),
            "Condicionamento", MatriculaRestricoes.None, laudos);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(2, resultado.Value!.LaudosMedicos.Count);
        Assert.Equal(new[] { "parecer.pdf", "exame.png" }, resultado.Value.LaudosMedicos.Select(laudo => laudo.NomeArquivo));
    }
}
