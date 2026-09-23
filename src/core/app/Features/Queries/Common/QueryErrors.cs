using DgiiEcf.Domain.Common.Results;

namespace DgiiEcf.Application.Features.Queries.Common;

public static class QueryErrors
{
    public static Error Required(string field) =>
        new($"query.{ToSnakeCase(field)}_required", $"El parámetro '{field}' es requerido.");

    private static string ToSnakeCase(string value) =>
        string.Concat(value.Select((character, index) =>
            index > 0 && char.IsUpper(character) ? "_" + char.ToLowerInvariant(character) : char.ToLowerInvariant(character).ToString()));
}
