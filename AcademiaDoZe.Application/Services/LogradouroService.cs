using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class LogradouroService : ILogradouroService
{
    private readonly ILogradouroRepository _repository;

    public LogradouroService(ILogradouroRepository repository)
    {
        _repository = repository;
    }

    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorId(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.ObterTodos(cancellationToken);
        return list.Select(l => l.ToDto());
    }

    public async Task<LogradouroDto> AdicionarAsync(LogradouroDto dto, CancellationToken cancellationToken = default)
    {
        var cepResult = Cep.Criar(dto.Cep);
        if (cepResult.IsFailure) throw new DomainException(string.Join(";", cepResult.Notifications.Select(n => n.Mensagem)));

        if (await _repository.CepJaExiste(cepResult.Value!, null, cancellationToken))
            throw new DomainException("CEP_JA_EXISTE");

        var entityResult = dto.ToEntity();
        if (entityResult.IsFailure) throw new DomainException(string.Join(";", entityResult.Notifications.Select(n => n.Mensagem)));

        var added = await _repository.Adicionar(entityResult.Value!, cancellationToken);
        return added.ToDto();
    }

    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto dto, CancellationToken cancellationToken = default)
    {
        var cepResult = Cep.Criar(dto.Cep);
        if (cepResult.IsFailure) throw new DomainException(string.Join(";", cepResult.Notifications.Select(n => n.Mensagem)));

        if (await _repository.CepJaExiste(cepResult.Value!, dto.Id, cancellationToken))
            throw new DomainException("CEP_JA_EXISTE");

        var entityResult = dto.ToEntity();
        if (entityResult.IsFailure) throw new DomainException(string.Join(";", entityResult.Notifications.Select(n => n.Mensagem)));

        var updated = await _repository.Atualizar(entityResult.Value!, cancellationToken);
        return updated.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.Remover(id, cancellationToken);
    }

    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure) throw new DomainException(string.Join(";", cepResult.Notifications.Select(n => n.Mensagem)));
        var entity = await _repository.ObterPorCep(cepResult.Value!, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<bool> CepJaExisteAsync(string cep, int? id = null, CancellationToken cancellationToken = default)
    {
        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure) throw new DomainException(string.Join(";", cepResult.Notifications.Select(n => n.Mensagem)));
        return await _repository.CepJaExiste(cepResult.Value!, id, cancellationToken);
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        var list = await _repository.ObterPorCidade(cidade, cancellationToken);
        return list.Select(l => l.ToDto());
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        var list = await _repository.ObterPorBairro(cidade, bairro, cancellationToken);
        return list.Select(l => l.ToDto());
    }
}
