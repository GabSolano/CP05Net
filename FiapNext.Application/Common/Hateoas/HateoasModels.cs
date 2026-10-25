namespace FiapNext.Application.Common.Hateoas;

public record LinkDto(string Href, string Rel, string Method);

public record ResourceEnvelope<T>(T Data, IReadOnlyList<LinkDto> Links);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Skip,
    int Limit,
    long TotalItems,
    int TotalPages,
    int CurrentPage,
    IReadOnlyList<LinkDto> Links
);
