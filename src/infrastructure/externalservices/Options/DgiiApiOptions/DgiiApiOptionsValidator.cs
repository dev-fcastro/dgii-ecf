using Microsoft.Extensions.Options;

namespace DgiiEcf.ExternalServices.Options.DgiiApiOptions;

public sealed class DgiiApiOptionsValidator : IValidateOptions<DgiiApiOptions>
{
    public ValidateOptionsResult Validate(string? name, DgiiApiOptions options)
    {
        var failures = new List<string>();

        foreach (var (key, value) in new[]
                 {
                     (nameof(options.EcfBaseUrl), options.EcfBaseUrl),
                     (nameof(options.FcBaseUrl), options.FcBaseUrl),
                     (nameof(options.StatusBaseUrl), options.StatusBaseUrl),
                 })
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                failures.Add($"{DgiiApiOptions.SectionName}:{key} debe ser una URL absoluta.");
            }
        }

        if (!Enum.IsDefined(options.Environment))
        {
            failures.Add($"{DgiiApiOptions.SectionName}:Environment debe ser Test, Certification o Production.");
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            failures.Add($"{DgiiApiOptions.SectionName}:Timeout debe ser mayor que cero.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
