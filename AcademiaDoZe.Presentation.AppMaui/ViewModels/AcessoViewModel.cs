using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class AcessoViewModel : BaseViewModel
{
    private readonly IAlunoRepository _alunos;
    private readonly IColaboradorRepository _colaboradores;
    private readonly IMatriculaRepository _matriculas;
    private readonly IAcessoAlunoRepository _acessosAlunos;
    private readonly IAcessoColaboradorRepository _acessosColaboradores;
    private string _cpf = string.Empty;
    private string _tipo = "Aluno";
    private string _mensagem = "Informe o CPF e registre a entrada ou saída.";

    public AcessoViewModel(IAlunoRepository alunos, IColaboradorRepository colaboradores, IMatriculaRepository matriculas,
        IAcessoAlunoRepository acessosAlunos, IAcessoColaboradorRepository acessosColaboradores)
    {
        _alunos = alunos;
        _colaboradores = colaboradores;
        _matriculas = matriculas;
        _acessosAlunos = acessosAlunos;
        _acessosColaboradores = acessosColaboradores;
        EntrarCommand = new Command(async () => await RegistrarAsync(true));
        SairCommand = new Command(async () => await RegistrarAsync(false));
    }

    public string Cpf { get => _cpf; set { _cpf = value; OnPropertyChanged(); } }
    public string Tipo { get => _tipo; set { _tipo = value; OnPropertyChanged(); } }
    public string Mensagem { get => _mensagem; private set { _mensagem = value; OnPropertyChanged(); } }
    public Command EntrarCommand { get; }
    public Command SairCommand { get; }

    private async Task RegistrarAsync(bool entrada)
    {
        if (IsBusy) return;
        IsBusy = true;
        Mensagem = string.Empty;
        try
        {
            var cpfResult = AcademiaDoZe.Domain.ValueObjects.Cpf.Criar(_cpf);
            if (cpfResult.IsFailure)
            {
                Mensagem = "CPF inválido.";
                return;
            }

            var agora = DateTime.Now;
            if (Tipo == "Aluno")
                await RegistrarAlunoAsync(cpfResult.Value!, entrada, agora);
            else
                await RegistrarColaboradorAsync(cpfResult.Value!, entrada, agora);
        }
        catch (Exception ex)
        {
            Mensagem = $"Não foi possível registrar o acesso. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegistrarAlunoAsync(Cpf cpf, bool entrada, DateTime agora)
    {
        var aluno = await _alunos.ObterPorCpf(cpf);
        if (aluno is null) { Mensagem = "Aluno não encontrado."; return; }

        var hoje = DateOnly.FromDateTime(agora);
        var acessos = (await _acessosAlunos.ObterAcessosPorAlunoPeriodo(aluno.Id, hoje, hoje)).OrderBy(a => a.DataHora).ToList();
        var estaDentro = acessos.Count % 2 == 1;

        if (entrada && estaDentro) { Mensagem = $"{aluno.Nome} já possui uma entrada registrada e ainda não realizou a saída."; return; }
        if (!entrada && !estaDentro) { Mensagem = $"{aluno.Nome} não possui entrada registrada para registrar a saída."; return; }

        if (entrada)
        {
            var matricula = await _matriculas.ObterMatriculaAtivaPorAluno(aluno.Id);
            if (matricula is null) { Mensagem = $"{aluno.Nome} não possui matrícula ativa."; return; }
            var diasRestantes = matricula.DataFim.DayNumber - hoje.DayNumber;
            Mensagem = $"Entrada registrada para {aluno.Nome}. Plano ativo por {Math.Max(0, diasRestantes)} dias até {matricula.DataFim:dd/MM/yyyy}.";
        }

        var novo = AcessoAluno.Criar(0, aluno.Id, agora);
        if (novo.IsFailure) { Mensagem = string.Join("; ", novo.Notifications.Select(n => n.Mensagem)); return; }
        await _acessosAlunos.Adicionar(novo.Value!);

        if (!entrada)
        {
            var permanencia = agora - acessos[^1].DataHora;
            Mensagem = $"Saída registrada para {aluno.Nome}. Permanência: {Formatar(permanencia)}.";
        }
    }

    private async Task RegistrarColaboradorAsync(Cpf cpf, bool entrada, DateTime agora)
    {
        var colaborador = await _colaboradores.ObterPorCpf(cpf);
        if (colaborador is null) { Mensagem = "Colaborador não encontrado."; return; }

        var hoje = DateOnly.FromDateTime(agora);
        var acessos = (await _acessosColaboradores.ObterAcessosPorColaboradorPeriodo(colaborador.Id, hoje, hoje)).OrderBy(a => a.DataHora).ToList();
        var estaDentro = acessos.Count % 2 == 1;

        if (entrada && estaDentro) { Mensagem = $"{colaborador.Nome} já possui uma entrada registrada e ainda não realizou a saída."; return; }
        if (!entrada && !estaDentro) { Mensagem = $"{colaborador.Nome} não possui entrada registrada para registrar a saída."; return; }

        var horas = await _acessosColaboradores.ObterHorasTrabalhadasNoDia(colaborador.Id, hoje);
        var limite = colaborador.Vinculo == ColaboradorVinculo.CLT ? TimeSpan.FromHours(8) : TimeSpan.FromHours(6);
        if (entrada && horas >= limite)
        {
            Mensagem = $"Limite diário atingido: {limite.TotalHours:0} horas para este vínculo.";
            return;
        }

        var novo = AcessoColaborador.Criar(0, colaborador.Id, agora);
        if (novo.IsFailure) { Mensagem = string.Join("; ", novo.Notifications.Select(n => n.Mensagem)); return; }
        await _acessosColaboradores.Adicionar(novo.Value!);

        if (entrada)
        {
            Mensagem = $"Entrada registrada para {colaborador.Nome}. Total acumulado hoje: {Formatar(horas)}.";
        }
        else
        {
            var permanencia = agora - acessos[^1].DataHora;
            var totalHoje = horas + permanencia;
            Mensagem = $"Saída registrada para {colaborador.Nome}. Permanência: {Formatar(permanencia)}. Total do dia: {Formatar(totalHoje)}.";
        }
    }

    private static string Formatar(TimeSpan tempo)
        => $"{(int)tempo.TotalHours:00}:{tempo.Minutes:00}";
}
