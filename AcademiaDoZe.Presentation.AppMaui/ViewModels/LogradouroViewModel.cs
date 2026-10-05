using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _service;

    public int Id { get; set; }
    public string Cep { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;

    public Command SaveCommand { get; }
    public Command DeleteCommand { get; }

    public LogradouroViewModel(ILogradouroService service)
    {
        _service = service;
        SaveCommand = new Command(async () => await SaveAsync());
        DeleteCommand = new Command(async () => await DeleteAsync());
    }

    public async Task SaveAsync()
    {
        IsBusy = true;
        var dto = new LogradouroDto(Id, Cep, Nome, Bairro, Cidade, Estado, Pais);
        if (Id == 0)
        {
            await _service.AdicionarAsync(dto);
        }
        else
        {
            await _service.AtualizarAsync(dto);
        }
        IsBusy = false;
        await Shell.Current.GoToAsync("..", true);
    }

    public async Task DeleteAsync()
    {
        if (Id == 0) return;
        await _service.RemoverAsync(Id);
        await Shell.Current.GoToAsync("..", true);
    }
}
