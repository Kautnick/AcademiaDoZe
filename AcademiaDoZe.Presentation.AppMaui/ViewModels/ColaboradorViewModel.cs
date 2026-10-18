using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Enums;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class ColaboradorViewModel : BaseViewModel
{
    private readonly IColaboradorService _service;
    private readonly ILogradouroService _logradouroService;
    private int _id;
    private string _nome = string.Empty;
    private string _cpf = string.Empty;
    private DateTime _dataNascimento = DateTime.Today.AddYears(-18);
    private DateTime _dataAdmissao = DateTime.Today;
    private string _telefone = string.Empty;
    private string _email = string.Empty;
    private string _numero = string.Empty;
    private string _complemento = string.Empty;
    private string _senha = string.Empty;
    private LogradouroDto? _enderecoSelecionado;
    private ColaboradorTipo _tipo = ColaboradorTipo.Instrutor;
    private ColaboradorVinculo _vinculo = ColaboradorVinculo.CLT;
    private byte[]? _foto;
    private string _mensagemErro = string.Empty;

    public ObservableCollection<LogradouroDto> Enderecos { get; } = [];
    public Array Tipos => Enum.GetValues<ColaboradorTipo>();
    public Array Vinculos => Enum.GetValues<ColaboradorVinculo>();
    public int Id
    {
        get => _id;
        private set
        {
            _id = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Titulo));
            OnPropertyChanged(nameof(HasId));
            OnPropertyChanged(nameof(PodeInformarSenha));
            OnPropertyChanged(nameof(PodeExcluir));
        }
    }
    public string Titulo => Id == 0 ? "Novo colaborador" : "Editar colaborador";
    public bool HasId => Id > 0;
    public bool PodeInformarSenha => Id == 0;
    public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(); LimparErro(); } }
    public string Cpf { get => _cpf; set { _cpf = value; OnPropertyChanged(); LimparErro(); } }
    public DateTime DataNascimento { get => _dataNascimento; set { _dataNascimento = value; OnPropertyChanged(); LimparErro(); } }
    public DateTime DataAdmissao { get => _dataAdmissao; set { _dataAdmissao = value; OnPropertyChanged(); LimparErro(); } }
    public string Telefone { get => _telefone; set { _telefone = value; OnPropertyChanged(); LimparErro(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); LimparErro(); } }
    public string Numero { get => _numero; set { _numero = value; OnPropertyChanged(); LimparErro(); } }
    public string Complemento { get => _complemento; set { _complemento = value; OnPropertyChanged(); } }
    public string Senha { get => _senha; set { _senha = value; OnPropertyChanged(); LimparErro(); } }
    public LogradouroDto? EnderecoSelecionado { get => _enderecoSelecionado; set { _enderecoSelecionado = value; OnPropertyChanged(); LimparErro(); } }
    public ColaboradorTipo Tipo { get => _tipo; set { _tipo = value; OnPropertyChanged(); LimparErro(); } }
    public ColaboradorVinculo Vinculo { get => _vinculo; set { _vinculo = value; OnPropertyChanged(); LimparErro(); } }
    public ImageSource? FotoSource => _foto is null ? null : ImageSource.FromStream(() => new MemoryStream(_foto));
    public bool TemFoto => _foto is { Length: > 0 };
    public bool NaoTemFoto => !TemFoto;
    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);
    public string MensagemErro { get => _mensagemErro; private set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); } }
    public bool PodeExcluir => Id > 0 && !IsBusy;
    public bool PodeSalvar => !IsBusy;

    public Command SaveCommand { get; }
    public Command DeleteCommand { get; }
    public Command CameraCommand { get; }
    public Command GalleryCommand { get; }

    public ColaboradorViewModel(IColaboradorService service, ILogradouroService logradouroService)
    {
        _service = service;
        _logradouroService = logradouroService;
        SaveCommand = new Command(async () => await SaveAsync());
        DeleteCommand = new Command(async () => await DeleteAsync());
        CameraCommand = new Command(async () => await CapturarFotoAsync());
        GalleryCommand = new Command(async () => await SelecionarFotoAsync());
    }

    public async Task InicializarAsync(int id)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var enderecos = await _logradouroService.ObterTodosAsync();
            Enderecos.Clear();
            foreach (var endereco in enderecos) Enderecos.Add(endereco);
            if (id <= 0) return;
            var dto = await _service.ObterPorIdAsync(id) ?? throw new InvalidOperationException("O colaborador selecionado não foi encontrado.");
            Id = dto.Id;
            Nome = dto.Nome;
            Cpf = dto.Cpf;
            DataNascimento = dto.DataNascimento.ToDateTime(TimeOnly.MinValue);
            DataAdmissao = dto.DataAdmissao.ToDateTime(TimeOnly.MinValue);
            Telefone = dto.Telefone;
            Email = dto.Email;
            Numero = dto.Numero;
            Complemento = dto.Complemento ?? string.Empty;
            EnderecoSelecionado = Enderecos.FirstOrDefault(e => e.Id == dto.Endereco.Id);
            Tipo = dto.Tipo;
            Vinculo = dto.Vinculo;
            _foto = dto.Foto?.Conteudo;
            NotificarFoto();
        }
        catch (Exception ex) { MensagemErro = $"Não foi possível carregar os dados do colaborador. {ex.Message}"; }
        finally { IsBusy = false; NotificarComandos(); }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        MensagemErro = string.Empty;
        if (EnderecoSelecionado is null) { MensagemErro = "Selecione um logradouro cadastrado."; return; }
        if (!TemFoto) { MensagemErro = "Adicione uma foto antes de salvar."; return; }
        if (Id == 0 && string.IsNullOrWhiteSpace(Senha)) { MensagemErro = "Informe uma senha para o colaborador."; return; }
        IsBusy = true;
        NotificarComandos();
        try
        {
            var dto = new ColaboradorDto(Id, Nome.Trim(), Cpf.Trim(), DateOnly.FromDateTime(DataNascimento), Telefone.Trim(), Email.Trim(), EnderecoSelecionado, Numero.Trim(), string.IsNullOrWhiteSpace(Complemento) ? null : Complemento.Trim(), new ArquivoDto(_foto!), DateOnly.FromDateTime(DataAdmissao), Tipo, Vinculo);
            if (Id == 0) await _service.AdicionarAsync(dto, Senha);
            else await _service.AtualizarAsync(dto);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { MensagemErro = TraduzirErro(ex.Message); }
        finally { IsBusy = false; NotificarComandos(); }
    }

    private async Task DeleteAsync()
    {
        var shell = Shell.Current;
        if (Id <= 0 || IsBusy || shell is null || !await shell.DisplayAlertAsync("Excluir colaborador", "Deseja realmente excluir este colaborador?", "Excluir", "Cancelar")) return;
        IsBusy = true;
        NotificarComandos();
        try
        {
            if (!await _service.RemoverAsync(Id)) { MensagemErro = "O colaborador não foi encontrado ou já foi removido."; return; }
            await shell.GoToAsync("..");
        }
        catch (Exception ex) { MensagemErro = $"Não foi possível excluir o colaborador. {ex.Message}"; }
        finally { IsBusy = false; NotificarComandos(); }
    }

    private async Task CapturarFotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported) { MensagemErro = "A câmera não está disponível neste dispositivo. Selecione uma foto da galeria."; return; }
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted) status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted) { MensagemErro = "A permissão da câmera não foi concedida."; return; }
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is not null) await LerFotoAsync(photo);
        }
        catch (OperationCanceledException) { }
        catch (FeatureNotSupportedException) { MensagemErro = "A câmera não está disponível neste dispositivo. Selecione uma foto da galeria."; }
        catch (PermissionException) { MensagemErro = "Não foi possível acessar a câmera. Verifique as permissões do dispositivo."; }
        catch (Exception ex) { MensagemErro = $"Não foi possível capturar a foto. {ex.Message}"; }
    }

    private async Task SelecionarFotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is not null) await LerFotoAsync(photo);
        }
        catch (OperationCanceledException) { }
        catch (FeatureNotSupportedException) { MensagemErro = "A seleção de fotos não é suportada neste dispositivo."; }
        catch (PermissionException) { MensagemErro = "Não foi possível acessar as fotos. Verifique as permissões do dispositivo."; }
        catch (Exception ex) { MensagemErro = $"Não foi possível ler a foto selecionada. {ex.Message}"; }
    }

    private async Task LerFotoAsync(FileResult arquivo)
    {
        await using var stream = await arquivo.OpenReadAsync();
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        int lidos;
        while ((lidos = await stream.ReadAsync(buffer)) > 0)
        {
            if (memory.Length + lidos > 15 * 1024 * 1024) { MensagemErro = "A foto deve ter no máximo 15 MB."; return; }
            await memory.WriteAsync(buffer.AsMemory(0, lidos));
        }
        if (memory.Length == 0) { MensagemErro = "O arquivo de foto está vazio ou não pôde ser lido."; return; }
        _foto = memory.ToArray();
        NotificarFoto();
        LimparErro();
    }

    private void NotificarFoto() { OnPropertyChanged(nameof(FotoSource)); OnPropertyChanged(nameof(TemFoto)); OnPropertyChanged(nameof(NaoTemFoto)); }
    private void LimparErro() { if (TemErro) MensagemErro = string.Empty; }
    private void NotificarComandos() { OnPropertyChanged(nameof(PodeExcluir)); OnPropertyChanged(nameof(PodeSalvar)); }
    private static string TraduzirErro(string mensagem) => string.Join(Environment.NewLine, mensagem.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(c => c switch
    {
        "ARQUIVO_OBRIGATORIO" => "Adicione uma foto.", "CPF_INVALIDO" => "Informe um CPF válido.", "CPF_JA_EXISTE" => "Já existe um colaborador com esse CPF.", "EMAIL_INVALIDO" => "Informe um e-mail válido.", "EMAIL_JA_EXISTE" => "Já existe um colaborador com esse e-mail.", "NOME_OBRIGATORIO" => "Informe o nome.", "DATA_NASCIMENTO_OBRIGATORIO" => "Informe a data de nascimento.", "DATA_NASCIMENTO_MINIMA_INVALIDA" => "O colaborador deve ter pelo menos 12 anos.", "DATA_ADMISSAO_OBRIGATORIO" => "Informe a data de admissão.", "DATA_ADMISSAO_MAIOR_QUE_ATUAL" => "A data de admissão não pode ser futura.", "TIPO_COLABORADOR_INVALIDO" => "Selecione um tipo válido.", "VINCULO_COLABORADOR_INVALIDO" => "Selecione um vínculo válido.", "ADMINISTRADOR_CLT_INVALIDO" => "Administrador deve possuir vínculo CLT.", "SENHA_FORMATO" => "A senha deve ter ao menos 6 caracteres e uma letra maiúscula.", "COLABORADOR_NAO_ENCONTRADO" => "O colaborador não foi encontrado.", _ => c
    }));
}
