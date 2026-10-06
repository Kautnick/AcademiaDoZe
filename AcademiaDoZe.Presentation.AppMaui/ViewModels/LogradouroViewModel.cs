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
    private string _mensagemErro = string.Empty;

    public int Id
    {
        get => _id;
        private set { _id = value; OnPropertyChanged(); OnPropertyChanged(nameof(PodeExcluir)); }
    }
    public bool PodeExcluir => Id > 0 && !IsBusy;
    public bool PodeSalvar => !IsBusy;
    public string MensagemErro
    {
        get => _mensagemErro;
        private set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); }
    }
    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);
    public string Cep { get => _cep; set { _cep = value; OnPropertyChanged(); LimparErro(); } }
    public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(); LimparErro(); } }
    public string Bairro { get => _bairro; set { _bairro = value; OnPropertyChanged(); LimparErro(); } }
    public string Cidade { get => _cidade; set { _cidade = value; OnPropertyChanged(); LimparErro(); } }
    public string Estado { get => _estado; set { _estado = value; OnPropertyChanged(); LimparErro(); } }
    public string Pais { get => _pais; set { _pais = value; OnPropertyChanged(); LimparErro(); } }

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
        NotificarEstadoDosComandos();
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
            MensagemErro = $"Não foi possível carregar o logradouro: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotificarEstadoDosComandos();
        }
    }

    public async Task SaveAsync()
    {
        if (IsBusy)
            return;

        MensagemErro = string.Empty;
        IsBusy = true;
        NotificarEstadoDosComandos();
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
            MensagemErro = TraduzirErro(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotificarEstadoDosComandos();
        }
    }

    public async Task DeleteAsync()
    {
        if (Id <= 0 || IsBusy)
            return;

        var shell = Shell.Current;
        if (shell is null || !await shell.DisplayAlertAsync("Excluir logradouro", "Deseja realmente excluir este logradouro?", "Excluir", "Cancelar"))
            return;

        MensagemErro = string.Empty;
        IsBusy = true;
        NotificarEstadoDosComandos();
        try
        {
            var removido = await _service.RemoverAsync(Id);
            if (!removido)
            {
                MensagemErro = "O logradouro não foi encontrado ou já foi removido.";
                return;
            }

            await shell.GoToAsync("..", true);
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível excluir o logradouro. Ele pode estar sendo usado por alunos ou colaboradores. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotificarEstadoDosComandos();
        }
    }

    private void LimparErro()
    {
        if (!string.IsNullOrEmpty(MensagemErro))
            MensagemErro = string.Empty;
    }

    private void NotificarEstadoDosComandos()
    {
        OnPropertyChanged(nameof(PodeExcluir));
        OnPropertyChanged(nameof(PodeSalvar));
    }

    private static string TraduzirErro(string mensagem)
    {
        var mensagens = mensagem.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(codigo => codigo switch
            {
                "CEP_OBRIGATORIO" => "Informe o CEP.",
                "CEP_DIGITOS" => "O CEP deve conter 8 dígitos. Ex.: 12345-678.",
                "CEP_JA_EXISTE" => "Já existe um logradouro cadastrado com esse CEP.",
                "NOME_OBRIGATORIO" => "Informe o nome do logradouro.",
                "BAIRRO_OBRIGATORIO" => "Informe o bairro.",
                "CIDADE_OBRIGATORIO" => "Informe a cidade.",
                "ESTADO_OBRIGATORIO" => "Informe a UF.",
                "PAIS_OBRIGATORIO" => "Informe o país.",
                _ => codigo
            })
            .Distinct();
        return string.Join(Environment.NewLine, mensagens);
    }
}
