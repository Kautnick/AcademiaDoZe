using System.Threading.Tasks;
using AcademiaDoZe.Application.Interfaces;
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

    public Command NavigateToLogradourosCommand { get; }

    public DashboardListViewModel(ILogradouroService logradouroService, IAlunoService alunoService, IColaboradorService colaboradorService, IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;

        Title = "Dashboard";
        NavigateToLogradourosCommand = new Command(async () => await Shell.Current.GoToAsync("//Logradouros"));
        Task.Run(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        var logradouros = await _logradouroService.ObterTodosAsync();
        var alunos = await _alunoService.ObterTodosAsync();
        var colaboradores = await _colaboradorService.ObterTodosAsync();
        var matriculas = await _matriculaService.ObterTodasAsync();

        LogradourosCount = logradouros?.Count() ?? 0;
        AlunosCount = alunos?.Count() ?? 0;
        ColaboradoresCount = colaboradores?.Count() ?? 0;
        MatriculasCount = matriculas?.Count() ?? 0;

        OnPropertyChanged(nameof(LogradourosCount));
        OnPropertyChanged(nameof(AlunosCount));
        OnPropertyChanged(nameof(ColaboradoresCount));
        OnPropertyChanged(nameof(MatriculasCount));
        IsBusy = false;
    }
}
