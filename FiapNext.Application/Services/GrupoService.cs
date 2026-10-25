using FiapNext.Application.DTOs;
using FiapNext.Application.Interfaces;
using FiapNext.Domain.Entities;
using FiapNext.Domain.Exceptions;
using FiapNext.Domain.Interfaces;

namespace FiapNext.Application.Services;

public class GrupoService : IGrupoService
{
    private readonly IGrupoRepository _grupoRepository;

    public GrupoService(IGrupoRepository grupoRepository)
    {
        _grupoRepository = grupoRepository;
    }

    public async Task<GrupoResponse> CreateAsync(CreateGrupoRequest request, CancellationToken ct = default)
    {
        var grupo = new Grupo(request.Nome, request.Turma, request.Integrantes);
        await _grupoRepository.CreateAsync(grupo, ct);
        return MapToResponse(grupo);
    }

    public async Task<GrupoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var grupo = await _grupoRepository.GetByIdAsync(id, ct);
        return grupo == null ? null : MapToResponse(grupo);
    }

    public async Task<IReadOnlyList<GrupoResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var grupos = await _grupoRepository.GetAllAsync(ct);
        return grupos.Select(MapToResponse).ToList();
    }

    public async Task<GrupoResponse> UpdateAsync(Guid id, UpdateGrupoRequest request, CancellationToken ct = default)
    {
        var grupo = await _grupoRepository.GetByIdAsync(id, ct);
        if (grupo == null)
            throw new KeyNotFoundException($"Grupo com ID {id} não foi encontrado.");

        grupo.Update(request.Nome, request.Turma, request.Integrantes);
        await _grupoRepository.UpdateAsync(grupo, ct);
        return MapToResponse(grupo);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _grupoRepository.DeleteAsync(id, ct);
    }

    private static GrupoResponse MapToResponse(Grupo grupo)
    {
        return new GrupoResponse(
            grupo.Id,
            grupo.Nome,
            grupo.Turma,
            grupo.Integrantes.AsReadOnly(),
            grupo.DataCriacao
        );
    }
}
