using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly IMatriculaRepository _matRepository;
    private readonly IAlunoRepository _alunoRepository;

    public MatriculaService(IMatriculaRepository matRepository, IAlunoRepository alunoRepository)
    {
        _matRepository = matRepository;
        _alunoRepository = alunoRepository;
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _matRepository.ObterPorId(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        var list = await _matRepository.ObterTodos(cancellationToken);
        return list.Select(m => m.ToDto());
    }

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto dto, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorId(dto.AlunoId, cancellationToken);
        if (aluno == null) throw new DomainException("ALUNO_NAO_ENCONTRADO");

        if (await _matRepository.PossuiMatriculaAtiva(dto.AlunoId, cancellationToken)) throw new DomainException("ALUNO_JA_POSSUI_MATRICULA_ATIVA");

        var entityResult = dto.ToEntity(aluno);
        if (entityResult.IsFailure) throw new DomainException(string.Join(";", entityResult.Notifications.Select(n => n.Mensagem)));

        var added = await _matRepository.Adicionar(entityResult.Value!, cancellationToken);
        return added.ToDto();
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto dto, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorId(dto.AlunoId, cancellationToken);
        if (aluno == null) throw new DomainException("ALUNO_NAO_ENCONTRADO");

        var matriculaAtiva = await _matRepository.ObterMatriculaAtivaPorAluno(dto.AlunoId, cancellationToken);
        if (matriculaAtiva is not null && matriculaAtiva.Id != dto.Id)
            throw new DomainException("ALUNO_JA_POSSUI_MATRICULA_ATIVA");

        var entityResult = dto.ToEntity(aluno);
        if (entityResult.IsFailure) throw new DomainException(string.Join(";", entityResult.Notifications.Select(n => n.Mensagem)));
        var updated = await _matRepository.Atualizar(entityResult.Value!, cancellationToken);
        return updated.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _matRepository.Remover(id, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var list = await _matRepository.ObterPorAluno(alunoId, cancellationToken);
        return list.Select(m => m.ToDto());
    }

    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var entity = await _matRepository.ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        return await _matRepository.PossuiMatriculaAtiva(alunoId, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(CancellationToken cancellationToken = default)
    {
        var list = await _matRepository.ObterAtivas(0, cancellationToken);
        return list.Select(m => m.ToDto());
    }

    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        var list = await _matRepository.ObterVencendoEmDias(dias, cancellationToken);
        return list.Select(m => m.ToDto());
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(Domain.Enums.MatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(plano)) throw new ArgumentOutOfRangeException(nameof(plano), "Plano de matrícula inválido.");
        var list = await _matRepository.ObterPorPlano(plano, cancellationToken);
        return list.Select(m => m.ToDto());
    }
}
