using System.Collections.ObjectModel;
using System.ComponentModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Enums;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class RestricaoMatriculaOpcao : INotifyPropertyChanged
{
    private readonly Action _onChanged;
    private bool _selecionada;

    public MatriculaRestricoes Valor { get; }
    public string Nome => ((Enum)Valor).GetDisplayName();
    public bool Selecionada
    {
        get => _selecionada;
        set
        {
            if (_selecionada == value) return;
            _selecionada = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selecionada)));
            _onChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public RestricaoMatriculaOpcao(MatriculaRestricoes valor, Action onChanged)
    {
        Valor = valor;
        _onChanged = onChanged;
    }
}

public sealed class LaudoMatriculaItem
{
    public string NomeArquivo { get; }
    public byte[] Conteudo { get; }

    public LaudoMatriculaItem(string nomeArquivo, byte[] conteudo)
    {
        NomeArquivo = nomeArquivo;
        Conteudo = conteudo;
    }
}

public sealed class MatriculaViewModel : BaseViewModel
{
    private const int TamanhoMaximoArquivo = 15 * 1024 * 1024;
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;
    private int _id;
    private string _cpf = string.Empty;
    private string _objetivo = string.Empty;
    private string _observacoesRestricoes = string.Empty;
    private DateTime _dataInicio = DateTime.Today;
    private MatriculaPlano _planoSelecionado = MatriculaPlano.Mensal;
    private AlunoDto? _alunoSelecionado;
    private string _mensagemErro = string.Empty;

    public Array Planos => Enum.GetValues<MatriculaPlano>();
    public ObservableCollection<RestricaoMatriculaOpcao> Restricoes { get; } = [];
    public ObservableCollection<LaudoMatriculaItem> Laudos { get; } = [];
    public int Id { get => _id; private set { _id = value; OnPropertyChanged(); OnPropertyChanged(nameof(Titulo)); } }
    public string Titulo => Id == 0 ? "Nova matrícula" : "Editar matrícula";
    public string Cpf { get => _cpf; set { _cpf = value; OnPropertyChanged(); LimparErro(); } }
    public string Objetivo { get => _objetivo; set { _objetivo = value; OnPropertyChanged(); OnPropertyChanged(nameof(ObjetivoPreview)); LimparErro(); } }
    public string ObservacoesRestricoes { get => _observacoesRestricoes; set { _observacoesRestricoes = value; OnPropertyChanged(); OnPropertyChanged(nameof(ObservacoesPreview)); } }
    public string ObjetivoPreview => string.IsNullOrWhiteSpace(Objetivo) ? "(não informado)" : Objetivo;
    public string ObservacoesPreview => string.IsNullOrWhiteSpace(ObservacoesRestricoes) ? "Nenhuma observação" : ObservacoesRestricoes;
    public string DataInicioFormatada => DataInicio.ToString("dd/MM/yyyy");
    public string PlanoDisplayName => ((Enum)PlanoSelecionado).GetDisplayName();
    public string ResumoRestricoes => ((Enum)RestricoesSelecionadas).GetDisplayName();
    public string ResumoLaudos => TemLaudo ? string.Join(", ", Laudos.Select(laudo => laudo.NomeArquivo)) : "Nenhum laudo anexado";
    public DateTime DataInicio
    {
        get => _dataInicio;
        set
        {
            _dataInicio = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DataFimCalculada));
            OnPropertyChanged(nameof(DataInicioFormatada));
            AtualizarObrigatoriedadeLaudo();
            LimparErro();
        }
    }
    public MatriculaPlano PlanoSelecionado
    {
        get => _planoSelecionado;
        set
        {
            _planoSelecionado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DataFimCalculada));
            OnPropertyChanged(nameof(PlanoDisplayName));
            LimparErro();
        }
    }
    public AlunoDto? AlunoSelecionado
    {
        get => _alunoSelecionado;
        private set
        {
            _alunoSelecionado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TemAlunoSelecionado));
            OnPropertyChanged(nameof(NomeAluno));
            OnPropertyChanged(nameof(CpfAluno));
            OnPropertyChanged(nameof(DataNascimentoAluno));
            OnPropertyChanged(nameof(FotoSource));
            AtualizarObrigatoriedadeLaudo();
        }
    }
    public bool TemAlunoSelecionado => AlunoSelecionado is not null;
    public string NomeAluno => AlunoSelecionado?.Nome ?? string.Empty;
    public string CpfAluno => AlunoSelecionado?.Cpf ?? string.Empty;
    public string DataNascimentoAluno => AlunoSelecionado?.DataNascimento.ToString("dd/MM/yyyy") ?? string.Empty;
    public ImageSource? FotoSource => AlunoSelecionado?.Foto?.Conteudo is { Length: > 0 } bytes
        ? ImageSource.FromStream(() => new MemoryStream(bytes))
        : null;
    public string DataFimCalculada
    {
        get
        {
            var inicio = DateOnly.FromDateTime(DataInicio);
            var fim = PlanoSelecionado switch
            {
                MatriculaPlano.Mensal => inicio.AddMonths(1),
                MatriculaPlano.Trimestral => inicio.AddMonths(3),
                MatriculaPlano.Semestral => inicio.AddMonths(6),
                MatriculaPlano.Anual => inicio.AddYears(1),
                _ => inicio
            };
            return fim.ToString("dd/MM/yyyy");
        }
    }
    public MatriculaRestricoes RestricoesSelecionadas => Restricoes
        .Where(opcao => opcao.Selecionada)
        .Aggregate(MatriculaRestricoes.None, (resultado, opcao) => resultado | opcao.Valor);
    public bool RequerLaudoMedico
    {
        get
        {
            if (RestricoesSelecionadas != MatriculaRestricoes.None) return true;
            if (AlunoSelecionado is null) return false;
            var nascimento = AlunoSelecionado.DataNascimento;
            var inicio = DateOnly.FromDateTime(DataInicio);
            var idade = inicio.Year - nascimento.Year;
            if (nascimento.AddYears(idade) > inicio) idade--;
            return idade is >= 12 and <= 16;
        }
    }
    public bool TemLaudo => Laudos.Count > 0;
    public string SituacaoLaudo => RequerLaudoMedico
        ? TemLaudo ? $"Laudo obrigatório — {Laudos.Count} documento(s) anexado(s)" : "Laudo médico obrigatório"
        : TemLaudo ? $"Laudo opcional — {Laudos.Count} documento(s) anexado(s)" : "Laudo opcional";
    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);
    public string MensagemErro { get => _mensagemErro; private set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); } }
    public bool PodeSalvar => !IsBusy;

    public Command BuscarAlunoCommand { get; }
    public Command SelecionarLaudoCommand { get; }
    public Command CapturarLaudoCommand { get; }
    public Command RemoverLaudoCommand { get; }
    public Command<LaudoMatriculaItem> RemoverLaudoItemCommand { get; }
    public Command SaveCommand { get; }
    public Command CancelCommand { get; }

    public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        foreach (var restricao in Enum.GetValues<MatriculaRestricoes>().Where(valor => valor != MatriculaRestricoes.None))
            Restricoes.Add(new RestricaoMatriculaOpcao(restricao, AtualizarObrigatoriedadeLaudo));

        BuscarAlunoCommand = new Command(async () => await BuscarAlunoAsync());
        SelecionarLaudoCommand = new Command(async () => await SelecionarLaudoAsync());
        CapturarLaudoCommand = new Command(async () => await CapturarLaudoAsync());
        RemoverLaudoCommand = new Command(RemoverLaudo);
        RemoverLaudoItemCommand = new Command<LaudoMatriculaItem>(RemoverLaudo);
        Laudos.CollectionChanged += (_, _) => NotificarLaudo();
        SaveCommand = new Command(async () => await SalvarAsync());
        CancelCommand = new Command(async () => await CancelarAsync());
    }

    public async Task InicializarAsync(int id)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            if (id <= 0) return;
            var matricula = await _matriculaService.ObterPorIdAsync(id)
                ?? throw new InvalidOperationException("A matrícula selecionada não foi encontrada.");
            var aluno = await _alunoService.ObterPorIdAsync(matricula.AlunoId)
                ?? throw new InvalidOperationException("O aluno vinculado à matrícula não foi encontrado.");

            Id = matricula.Id;
            Cpf = aluno.Cpf;
            AlunoSelecionado = aluno;
            PlanoSelecionado = matricula.Plano;
            DataInicio = matricula.DataInicio.ToDateTime(TimeOnly.MinValue);
            Objetivo = matricula.Objetivo;
            ObservacoesRestricoes = matricula.ObservacoesRestricoes;
            foreach (var opcao in Restricoes)
                opcao.Selecionada = matricula.RestricoesMedicas.HasFlag(opcao.Valor);
            Laudos.Clear();
            foreach (var laudo in matricula.LaudosMedicos)
                Laudos.Add(new LaudoMatriculaItem(laudo.NomeArquivo, laudo.Conteudo));
            NotificarLaudo();
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível carregar a matrícula. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotificarEstado();
        }
    }

    private async Task BuscarAlunoAsync()
    {
        if (IsBusy) return;
        MensagemErro = string.Empty;
        if (string.IsNullOrWhiteSpace(Cpf))
        {
            MensagemErro = "Informe o CPF do aluno.";
            return;
        }

        IsBusy = true;
        try
        {
            var aluno = await _alunoService.ObterPorCpfAsync(Cpf.Trim());
            if (aluno is null)
            {
                AlunoSelecionado = null;
                MensagemErro = "Nenhum aluno foi encontrado com esse CPF.";
                return;
            }
            AlunoSelecionado = aluno;
            LimparErro();
        }
        catch (Exception ex)
        {
            MensagemErro = $"Não foi possível pesquisar o aluno. {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            NotificarEstado();
        }
    }

    private async Task SalvarAsync()
    {
        if (IsBusy) return;
        MensagemErro = string.Empty;
        if (AlunoSelecionado is null) { MensagemErro = "Pesquise e selecione um aluno válido pelo CPF."; return; }
        if (string.IsNullOrWhiteSpace(Objetivo)) { MensagemErro = "Informe o objetivo da matrícula."; return; }
        if (RequerLaudoMedico && !TemLaudo) { MensagemErro = "Anexe o laudo/parecer médico obrigatório para este aluno e suas condições de saúde."; return; }

        IsBusy = true;
        NotificarEstado();
        try
        {
            var inicio = DateOnly.FromDateTime(DataInicio);
            var dataFim = PlanoSelecionado switch
            {
                MatriculaPlano.Mensal => inicio.AddMonths(1),
                MatriculaPlano.Trimestral => inicio.AddMonths(3),
                MatriculaPlano.Semestral => inicio.AddMonths(6),
                MatriculaPlano.Anual => inicio.AddYears(1),
                _ => inicio
            };
            var dto = new MatriculaDto(
                Id,
                AlunoSelecionado.Id,
                PlanoSelecionado,
                inicio,
                dataFim,
                Objetivo.Trim(),
                RestricoesSelecionadas,
                ObservacoesRestricoes.Trim(),
                Laudos.Select(laudo => new MatriculaLaudoDto(laudo.NomeArquivo, laudo.Conteudo)).ToArray());

            if (Id == 0) await _matriculaService.AdicionarAsync(dto);
            else await _matriculaService.AtualizarAsync(dto);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            MensagemErro = TraduzirErro(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotificarEstado();
        }
    }

    private async Task SelecionarLaudoAsync()
    {
        try
        {
            var tipos = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".pdf", ".png", ".jpg", ".jpeg", ".webp"],
                [DevicePlatform.Android] = ["application/pdf", "image/*"],
                [DevicePlatform.iOS] = ["public.pdf", "public.image"],
                [DevicePlatform.MacCatalyst] = ["public.pdf", "public.image"]
            });
            var arquivos = await FilePicker.Default.PickMultipleAsync(new PickOptions { PickerTitle = "Selecione um ou mais laudos médicos", FileTypes = tipos });
            foreach (var arquivo in arquivos)
            {
                var extensao = Path.GetExtension(arquivo.FileName);
                if (!new[] { ".pdf", ".png", ".jpg", ".jpeg", ".webp" }.Contains(extensao, StringComparer.OrdinalIgnoreCase))
                {
                    MensagemErro = $"O arquivo {arquivo.FileName} não é um documento PDF ou uma imagem suportada.";
                    continue;
                }
                await ArmazenarLaudoAsync(arquivo, arquivo.FileName);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { MensagemErro = $"Não foi possível selecionar o laudo. {ex.Message}"; }
    }

    private async Task CapturarLaudoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                MensagemErro = "A câmera não está disponível neste dispositivo. Selecione uma imagem ou PDF.";
                return;
            }
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted) status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                MensagemErro = "A permissão para usar a câmera não foi concedida.";
                return;
            }
            var foto = await MediaPicker.Default.CapturePhotoAsync();
            if (foto is not null) await ArmazenarLaudoAsync(foto, foto.FileName);
        }
        catch (OperationCanceledException) { }
        catch (FeatureNotSupportedException) { MensagemErro = "A câmera não é suportada neste dispositivo."; }
        catch (PermissionException) { MensagemErro = "Não foi possível acessar a câmera. Verifique as permissões."; }
        catch (Exception ex) { MensagemErro = $"Não foi possível capturar o documento. {ex.Message}"; }
    }

    private async Task ArmazenarLaudoAsync(FileResult arquivo, string nome)
    {
        await using var stream = await arquivo.OpenReadAsync();
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        int lidos;
        while ((lidos = await stream.ReadAsync(buffer)) > 0)
        {
            if (memory.Length + lidos > TamanhoMaximoArquivo)
            {
                MensagemErro = "O laudo deve ter no máximo 15 MB.";
                return;
            }
            await memory.WriteAsync(buffer.AsMemory(0, lidos));
        }
        if (memory.Length == 0)
        {
            MensagemErro = "O documento está vazio ou não pôde ser lido.";
            return;
        }
        Laudos.Add(new LaudoMatriculaItem(nome, memory.ToArray()));
        OnPropertyChanged(nameof(ResumoLaudos));
        LimparErro();
    }

    private void RemoverLaudo()
    {
        Laudos.Clear();
        OnPropertyChanged(nameof(ResumoLaudos));
        LimparErro();
    }

    private void RemoverLaudo(LaudoMatriculaItem? laudo)
    {
        if (laudo is not null)
            Laudos.Remove(laudo);
        OnPropertyChanged(nameof(ResumoLaudos));
        LimparErro();
    }

    private async Task CancelarAsync()
    {
        try { await Shell.Current.GoToAsync(".."); }
        catch (Exception ex) { MensagemErro = $"Não foi possível cancelar a edição. {ex.Message}"; }
    }

    private void AtualizarObrigatoriedadeLaudo()
    {
        OnPropertyChanged(nameof(RestricoesSelecionadas));
        OnPropertyChanged(nameof(ResumoRestricoes));
        OnPropertyChanged(nameof(RequerLaudoMedico));
        OnPropertyChanged(nameof(SituacaoLaudo));
    }

    private void NotificarLaudo()
    {
        OnPropertyChanged(nameof(TemLaudo));
        OnPropertyChanged(nameof(SituacaoLaudo));
        OnPropertyChanged(nameof(RequerLaudoMedico));
        OnPropertyChanged(nameof(ResumoLaudos));
    }

    private void NotificarEstado()
    {
        OnPropertyChanged(nameof(PodeSalvar));
        SaveCommand.ChangeCanExecute();
    }

    private void LimparErro()
    {
        if (TemErro) MensagemErro = string.Empty;
    }

    private static string TraduzirErro(string mensagem) => string.Join(Environment.NewLine,
        mensagem.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(codigo => codigo switch
        {
            "ALUNO_NAO_ENCONTRADO" => "O aluno selecionado não foi encontrado.",
            "ALUNO_JA_POSSUI_MATRICULA_ATIVA" => "Este aluno já possui uma matrícula ativa.",
            "LAUDO_12_A_16_OBRIGATORIO" => "Alunos de 12 a 16 anos precisam apresentar laudo médico.",
            "RESTRICOES_LAUDO_OBRIGATORIO" => "As restrições de saúde exigem laudo médico.",
            "PLANO_INVALIDO" => "Selecione um plano válido.",
            "DATA_INICIO_OBRIGATORIO" => "Informe a data de início.",
            "OBJETIVO_OBRIGATORIO" => "Informe o objetivo da matrícula.",
            _ => codigo
        }));
}
