using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Presentation.AppMaui;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class MatriculaListItem
{
    public MatriculaDto Matricula { get; }
    public AlunoDto Aluno { get; }
    public int MatriculaId => Matricula.Id;
    public int AlunoId => Aluno.Id;
    public string NomeAluno => Aluno.Nome;
    public string Cpf => Aluno.Cpf;
    public MatriculaPlano Plano => Matricula.Plano;
    public DateOnly DataInicio => Matricula.DataInicio;
    public DateOnly DataFim => Matricula.DataFim;
    public string Objetivo => Matricula.Objetivo;
    public MatriculaRestricoes RestricoesMedicas => Matricula.RestricoesMedicas;
    public string ObservacoesRestricoes => Matricula.ObservacoesRestricoes;
    public bool TemObservacoes => !string.IsNullOrWhiteSpace(Matricula.ObservacoesRestricoes);
    public bool TemFoto => Aluno.Foto?.Conteudo is { Length: > 0 };
    public ImageSource? FotoSource => Aluno.Foto?.Conteudo is { Length: > 0 } bytes
        ? ImageSource.FromStream(() => new MemoryStream(bytes))
        : null;

    public MatriculaListItem(MatriculaDto matricula, AlunoDto aluno)
    {
        Matricula = matricula;
        Aluno = aluno;
    }
}

public sealed class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;
    private string _idMatricula = string.Empty;
    private string _idAluno = string.Empty;
    private string _cpf = string.Empty;
    private string _diasParaVencer = string.Empty;
    private string _mensagemErro = string.Empty;
    private bool _apenasAtivas;

    public ObservableCollection<MatriculaListItem> Items { get; } = [];
    public string IdMatricula { get => _idMatricula; set { _idMatricula = value; OnPropertyChanged(); } }
    public string IdAluno { get => _idAluno; set { _idAluno = value; OnPropertyChanged(); } }
    public string Cpf { get => _cpf; set { _cpf = value; OnPropertyChanged(); } }
    public string DiasParaVencer { get => _diasParaVencer; set { _diasParaVencer = value; OnPropertyChanged(); } }
    public bool ApenasAtivas { get => _apenasAtivas; set { _apenasAtivas = value; OnPropertyChanged(); } }
    public string MensagemErro { get => _mensagemErro; private set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); } }
    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);
    public bool HasNoItems => Items.Count == 0 && !IsBusy;

    public Command SearchCommand { get; }
    public Command AddCommand { get; }
    public Command RefreshCommand { get; }

    public MatriculaListViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        SearchCommand = new Command(async () => await LoadAsync());
        RefreshCommand = new Command(async () => await LoadAsync());
        AddCommand = new Command(async () => await NavegarAsync(AppShell.MatriculaFormRoute));
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        MensagemErro = string.Empty;
        OnPropertyChanged(nameof(HasNoItems));
        try
        {
            var alunos = (await _alunoService.ObterTodosAsync()).ToDictionary(aluno => aluno.Id);
            IEnumerable<MatriculaDto> matriculas;
            if (!string.IsNullOrWhiteSpace(DiasParaVencer))
            {
                if (!int.TryParse(DiasParaVencer, out var dias) || dias < 0)
                {
                    MensagemErro = "Informe uma quantidade de dias válida (zero ou maior).";
                    return;
                }
                matriculas = await _matriculaService.ObterVencendoEmDiasAsync(dias);
            }
            else if (ApenasAtivas)
            {
                matriculas = await _matriculaService.ObterAtivasAsync();
            }
            else
            {
                matriculas = await _matriculaService.ObterTodasAsync();
            }

            var itens = matriculas
                .Where(matricula => alunos.ContainsKey(matricula.AlunoId))
                .Select(matricula => new MatriculaListItem(matricula, alunos[matricula.AlunoId]));

            if (!string.IsNullOrWhiteSpace(IdMatricula))
            {
                if (!int.TryParse(IdMatricula, out var id))
                {
                    MensagemErro = "Informe um ID de matrícula válido.";
                    return;
                }
                itens = itens.Where(item => item.MatriculaId == id);
            }

            if (!string.IsNullOrWhiteSpace(IdAluno))
            {
                if (!int.TryParse(IdAluno, out var id))
                {
                    MensagemErro = "Informe um ID de aluno válido.";
                    return;
                }
                itens = itens.Where(item => item.AlunoId == id);
            }

            if (!string.IsNullOrWhiteSpace(Cpf))
            {
                var cpf = new string(Cpf.Where(char.IsDigit).ToArray());
                if (cpf.Length == 0)
                {
                    MensagemErro = "Informe ao menos um dígito para pesquisar o CPF.";
                    return;
                }
                itens = itens.Where(item => new string(item.Cpf.Where(char.IsDigit).ToArray()).Contains(cpf, StringComparison.Ordinal));
            }

            Items.Clear();
            foreach (var item in itens.OrderByDescending(item => item.DataInicio))
                Items.Add(item);
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível carregar as matrículas. Confira a conexão com o SQL Server. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasNoItems));
        }
    }

    public Task AbrirAsync(int id) => NavegarAsync($"{AppShell.MatriculaFormRoute}?id={id}");

    public async Task ExcluirAsync(MatriculaListItem item)
    {
        var shell = Shell.Current;
        if (shell is null || IsBusy) return;
        if (!await shell.DisplayAlertAsync("Excluir matrícula", $"Deseja realmente excluir a matrícula {item.MatriculaId} de {item.NomeAluno}?", "Excluir", "Cancelar")) return;

        IsBusy = true;
        MensagemErro = string.Empty;
        try
        {
            if (!await _matriculaService.RemoverAsync(item.MatriculaId))
                throw new InvalidOperationException("A matrícula não foi encontrada ou já foi removida.");
            await LoadAsyncAfterDelete();
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível excluir a matrícula. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasNoItems));
        }
    }

    private async Task LoadAsyncAfterDelete()
    {
        IsBusy = false;
        await LoadAsync();
    }

    private static async Task NavegarAsync(string rota)
    {
        try
        {
            await Shell.Current.GoToAsync(rota);
        }
        catch (Exception ex)
        {
            if (Shell.Current is { } shell)
                await shell.DisplayAlertAsync("Academia do Zé", $"Não foi possível abrir a tela: {ex.Message}", "OK");
        }
    }
}
