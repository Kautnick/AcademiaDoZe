using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _service;
    private string _query = string.Empty;
    private string _mensagemErro = string.Empty;
    private List<ColaboradorDto> _todos = [];

    public ObservableCollection<ColaboradorDto> Items { get; } = [];
    public string Query { get => _query; set { _query = value; OnPropertyChanged(); } }
    public string MensagemErro { get => _mensagemErro; private set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); } }
    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);
    public bool HasNoItems => Items.Count == 0 && !IsBusy;
    public Command SearchCommand { get; }
    public Command AddCommand { get; }
    public Command RefreshCommand { get; }

    public ColaboradorListViewModel(IColaboradorService service)
    {
        _service = service;
        SearchCommand = new Command(AplicarFiltro);
        AddCommand = new Command(async () => await NavegarAsync(AppShell.ColaboradorFormRoute));
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        MensagemErro = string.Empty;
        OnPropertyChanged(nameof(HasNoItems));
        try
        {
            _todos = (await _service.ObterTodosAsync()).ToList();
            AplicarFiltro();
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível carregar os colaboradores. Confira a conexão com o SQL Server. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasNoItems));
        }
    }

    public async Task AbrirAsync(ColaboradorDto colaborador)
    {
        if (colaborador is not null)
            await NavegarAsync($"{AppShell.ColaboradorFormRoute}?id={colaborador.Id}");
    }

    private void AplicarFiltro()
    {
        var termo = new string(Query.Where(char.IsDigit).ToArray());
        var filtrados = string.IsNullOrWhiteSpace(Query)
            ? _todos
            : string.IsNullOrEmpty(termo)
                ? Enumerable.Empty<ColaboradorDto>()
                : _todos.Where(colaborador => new string(colaborador.Cpf.Where(char.IsDigit).ToArray()).Contains(termo, StringComparison.Ordinal));
        Items.Clear();
        foreach (var colaborador in filtrados)
            Items.Add(colaborador);
        OnPropertyChanged(nameof(HasNoItems));
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
