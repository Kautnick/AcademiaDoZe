using System.Threading.Tasks;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _service;
    private int _id;
    private string _cep = string.Empty;
    private string _nome = string.Empty;
    private string _bairro = string.Empty;
    private string _cidade = string.Empty;
    private string _estado = string.Empty;
    private string _pais = string.Empty;

    public int Id
    {
        get => _id;
        private set { _id = value; OnPropertyChanged(); OnPropertyChanged(nameof(PodeExcluir)); }
    }
    public bool PodeExcluir => Id > 0 && !IsBusy;
    public string Cep { get => _cep; set { _cep = value; OnPropertyChanged(); } }
    public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(); } }
    public string Bairro { get => _bairro; set { _bairro = value; OnPropertyChanged(); } }
    public string Cidade { get => _cidade; set { _cidade = value; OnPropertyChanged(); } }
    public string Estado { get => _estado; set { _estado = value; OnPropertyChanged(); } }
    public string Pais { get => _pais; set { _pais = value; OnPropertyChanged(); } }

    public Command SaveCommand { get; }
    public Command DeleteCommand { get; }

    public LogradouroViewModel(ILogradouroService service)
    {
        _service = service;
        SaveCommand = new Command(async () => await SaveAsync());
        DeleteCommand = new Command(async () => await DeleteAsync());
    }

    public async Task CarregarAsync(int id)
    {
        if (id <= 0 || IsBusy)
            return;

        IsBusy = true;
        OnPropertyChanged(nameof(PodeExcluir));
        try
        {
            var logradouro = await _service.ObterPorIdAsync(id);
            if (logradouro is null)
                throw new InvalidOperationException("O logradouro selecionado não foi encontrado.");

            Id = logradouro.Id;
            Cep = logradouro.Cep;
            Nome = logradouro.Nome;
            Bairro = logradouro.Bairro;
            Cidade = logradouro.Cidade;
            Estado = logradouro.Estado;
            Pais = logradouro.Pais;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Não foi possível carregar o logradouro: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(PodeExcluir));
        }
    }

    public async Task SaveAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        OnPropertyChanged(nameof(PodeExcluir));
        try
        {
            var dto = new LogradouroDto(Id, Cep, Nome, Bairro, Cidade, Estado, Pais);
            if (Id == 0)
                await _service.AdicionarAsync(dto);
            else
                await _service.AtualizarAsync(dto);

            await Shell.Current.GoToAsync("..", true);
        }
        catch (Exception ex)
        {
            var message = ex.Message.Contains("CEP_DIGITOS", StringComparison.Ordinal)
                ? "O CEP deve conter exatamente 8 dígitos. Ex.: 12345678 ou 12345-678."
                : ex.Message;
            await ShowErrorAsync(message);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(PodeExcluir));
        }
    }

    public async Task DeleteAsync()
    {
        if (Id <= 0 || IsBusy)
            return;

        var shell = Shell.Current;
        if (shell is null || !await shell.DisplayAlert("Excluir logradouro", "Deseja realmente excluir este logradouro?", "Excluir", "Cancelar"))
            return;

        IsBusy = true;
        OnPropertyChanged(nameof(PodeExcluir));
        try
        {
            var removido = await _service.RemoverAsync(Id);
            if (!removido)
            {
                await ShowErrorAsync("O logradouro não foi encontrado ou já foi removido.");
                return;
            }

            await shell.GoToAsync("..", true);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Não foi possível excluir o logradouro. Ele pode estar sendo usado por alunos ou colaboradores. {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(PodeExcluir));
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
