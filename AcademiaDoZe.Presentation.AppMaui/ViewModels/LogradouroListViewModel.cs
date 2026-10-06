using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _service;
    public ObservableCollection<LogradouroDto> Items { get; } = new();
    public bool HasNoItems => Items.Count == 0 && !IsBusy;

    private string? _query;
    public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }
    private string _mensagemErro = string.Empty;
    public string MensagemErro
    {
        get => _mensagemErro;
        private set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); }
    }
    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

    public Command SearchCommand { get; }
    public Command AddCommand { get; }
    public Command RefreshCommand { get; }

    public LogradouroListViewModel(ILogradouroService service)
    {
        _service = service;
        SearchCommand = new Command(async () => await SearchAsync());
        AddCommand = new Command(async () =>
        {
            try
            {
                await Shell.Current.GoToAsync(AppShell.LogradouroFormRoute);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync(ex.Message);
            }
        });
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        MensagemErro = string.Empty;
        OnPropertyChanged(nameof(HasNoItems));
        try
        {
            var list = await _service.ObterTodosAsync();
            AtualizarItens(list);
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível carregar os logradouros. Confira a conexão com o SQL Server. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasNoItems));
        }
    }

    public async Task SearchAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        MensagemErro = string.Empty;
        OnPropertyChanged(nameof(HasNoItems));
        try
        {
            var list = await _service.ObterTodosAsync();
            var termo = Query?.Trim();
            if (!string.IsNullOrEmpty(termo))
            {
                var cepTermo = new string(termo.Where(char.IsDigit).ToArray());
                list = list.Where(item =>
                    (!string.IsNullOrEmpty(cepTermo) && item.Cep.Contains(cepTermo, StringComparison.OrdinalIgnoreCase))
                    || item.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                    || item.Cidade.Contains(termo, StringComparison.OrdinalIgnoreCase));
            }

            AtualizarItens(list);
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível buscar os logradouros. Confira a conexão com o SQL Server. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasNoItems));
        }
    }

    private void AtualizarItens(IEnumerable<LogradouroDto> logradouros)
    {
        Items.Clear();
        foreach (var item in logradouros)
            Items.Add(item);
        OnPropertyChanged(nameof(HasNoItems));
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
