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

    private string? _query;
    public string? Query { get => _query; set { _query = value; OnPropertyChanged(); } }

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
        IsBusy = true;
        try
        {
            var list = await _service.ObterTodosAsync();
            Items.Clear();
            foreach (var item in list) Items.Add(item);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Não foi possível carregar os logradouros. Verifique a conexão com o SQL Server. {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SearchAsync()
    {
        IsBusy = true;
        try
        {
            IEnumerable<LogradouroDto> list;
            if (string.IsNullOrWhiteSpace(Query)) list = await _service.ObterTodosAsync();
            else list = await _service.ObterPorCidadeAsync(Query);
            Items.Clear();
            foreach (var item in list) Items.Add(item);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Não foi possível buscar os logradouros. Verifique a conexão com o SQL Server. {ex.Message}");
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
                await shell.DisplayAlert("Academia do Zé", message, "OK");
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
