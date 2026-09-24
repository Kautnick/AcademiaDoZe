using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _repository;
    private readonly IPasswordHasher _hasher;

    public AlunoService(IAlunoRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorId(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.ObterTodos(cancellationToken);
        return list.Select(a => a.ToDto());
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto dto, string senha, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(dto.Cpf);
        if (cpfResult.IsFailure) throw new DomainException(string.Join(";", cpfResult.Notifications.Select(n => n.Mensagem)));
        if (await _repository.CpfJaExiste(cpfResult.Value!, null, cancellationToken)) throw new DomainException("CPF_JA_EXISTE");

        var emailResult = Email.Criar(dto.Email);
        if (emailResult.IsFailure) throw new DomainException(string.Join(";", emailResult.Notifications.Select(n => n.Mensagem)));
        if (await _repository.EmailJaExiste(emailResult.Value!, null, cancellationToken)) throw new DomainException("EMAIL_JA_EXISTE");

        // Hash da senha antes de criar a entidade
        var hashed = _hasher.Hash(senha);

        var entityResult = dto.ToEntity(hashed);
        if (entityResult.IsFailure) throw new DomainException(string.Join(";", entityResult.Notifications.Select(n => n.Mensagem)));

        var added = await _repository.Adicionar(entityResult.Value!, cancellationToken);
        return added.ToDto();
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto dto, CancellationToken cancellationToken = default)
    {
        var entityResult = dto.ToEntity("");
        if (entityResult.IsFailure) throw new DomainException(string.Join(";", entityResult.Notifications.Select(n => n.Mensagem)));
        var updated = await _repository.Atualizar(entityResult.Value!, cancellationToken);
        return updated.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.Remover(id, cancellationToken);
    }

    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) throw new DomainException(string.Join(";", cpfResult.Notifications.Select(n => n.Mensagem)));
        var entity = await _repository.ObterPorCpf(cpfResult.Value!, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) throw new DomainException(string.Join(";", emailResult.Notifications.Select(n => n.Mensagem)));
        var entity = await _repository.ObterPorEmail(emailResult.Value!, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var list = await _repository.ObterPorNome(nome, cancellationToken);
        return list.Select(a => a.ToDto());
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) throw new DomainException(string.Join(";", cpfResult.Notifications.Select(n => n.Mensagem)));
        return await _repository.CpfJaExiste(cpfResult.Value!, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) throw new DomainException(string.Join(";", emailResult.Notifications.Select(n => n.Mensagem)));
        return await _repository.EmailJaExiste(emailResult.Value!, id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        var hashed = _hasher.Hash(novaSenha);
        var senhaResult = Senha.Criar(hashed);
        if (senhaResult.IsFailure) throw new DomainException(string.Join(";", senhaResult.Notifications.Select(n => n.Mensagem)));
        return await _repository.TrocarSenha(id, senhaResult.Value!, cancellationToken);
    }
}
