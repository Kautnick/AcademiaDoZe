using System.Threading.Tasks;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class DashboardListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    public int LogradourosCount { get; private set; }
    public int AlunosCount { get; private set; }
    public int ColaboradoresCount { get; private set; }
    public int MatriculasCount { get; private set; }
    private string _loadErrorMessage = string.Empty;
    public string LoadErrorMessage
    {
        get => _loadErrorMessage;
        private set
        {
            _loadErrorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasLoadError));
        }
    }
    public bool HasLoadError => !string.IsNullOrWhiteSpace(LoadErrorMessage);

    public Command NavigateToLogradourosCommand { get; }

    public DashboardListViewModel(ILogradouroService logradouroService, IAlunoService alunoService, IColaboradorService colaboradorService, IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;

        Title = "Dashboard";
        NavigateToLogradourosCommand = new Command(async () =>
        {
            try
            {
                await Shell.Current.GoToAsync("//Logradouros");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        });
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        LoadErrorMessage = string.Empty;
        try
        {
            var logradouros = await _logradouroService.ObterTodosAsync();
            var alunos = await _alunoService.ObterTodosAsync();
            var colaboradores = await _colaboradorService.ObterTodosAsync();
            var matriculas = await _matriculaService.ObterTodasAsync();

            LogradourosCount = logradouros.Count();
            AlunosCount = alunos.Count();
            ColaboradoresCount = colaboradores.Count();
            MatriculasCount = matriculas.Count();

            OnPropertyChanged(nameof(LogradourosCount));
            OnPropertyChanged(nameof(AlunosCount));
            OnPropertyChanged(nameof(ColaboradoresCount));
            OnPropertyChanged(nameof(MatriculasCount));
        }
        catch (Exception ex)
        {
            LoadErrorMessage = $"Não foi possível atualizar os dados. Confira a conexão com o SQL Server. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task ShowErrorAsync(string message)
    {
        try
        {
            if (Shell.Current is { } shell)
                await shell.DisplayAlertAsync("Academia do Zé", message, "OK");
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
