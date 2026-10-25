using FiapNext.Application.Common.Hateoas;
using FiapNext.Application.DTOs;

namespace FiapNext.Application.Interfaces;

public interface IHateoasService
{
    ResourceEnvelope<TrabalhoResponse> EnvelopTrabalho(TrabalhoResponse trabalho, bool isAdmin, string baseUrl);
    PagedResult<ResourceEnvelope<TrabalhoResponse>> EnvelopTrabalhosPaged(
        IReadOnlyList<TrabalhoResponse> itens,
        TrabalhoFilterParams filter,
        long totalItems,
        bool isAdmin,
        string baseUrl);
    ResourceEnvelope<GrupoResponse> EnvelopGrupo(GrupoResponse grupo, bool isAdmin, string baseUrl);
}
