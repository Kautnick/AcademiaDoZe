using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
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
        AddCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(Views.LogradouroPage)));
        RefreshCommand = new Command(async () => await LoadAsync());
        Task.Run(async () => await LoadAsync());
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        Items.Clear();
        var list = await _service.ObterTodosAsync();
        foreach (var item in list) Items.Add(item);
        IsBusy = false;
    }

    public async Task SearchAsync()
    {
        IsBusy = true;
        IEnumerable<LogradouroDto> list;
        if (string.IsNullOrWhiteSpace(Query)) list = await _service.ObterTodosAsync();
        else list = await _service.ObterPorCidadeAsync(Query);
        Items.Clear();
        foreach (var item in list) Items.Add(item);
        IsBusy = false;
    }
}
