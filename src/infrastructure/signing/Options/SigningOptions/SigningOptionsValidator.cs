using Microsoft.Extensions.Options;

namespace DgiiEcf.Signing.Options.SigningOptions;

public sealed class SigningOptionsValidator : IValidateOptions<SigningOptions>
{
    public ValidateOptionsResult Validate(string? name, SigningOptions options)
    {
        var hasPath = !string.IsNullOrWhiteSpace(options.CertificatePath);
        var hasBase64 = !string.IsNullOrWhiteSpace(options.CertificateBase64);

        if (!hasPath && !hasBase64)
        {
            return ValidateOptionsResult.Fail($"{SigningOptions.SectionName}: indique CertificatePath o CertificateBase64.");
        }

        if (hasPath && hasBase64)
        {
            return ValidateOptionsResult.Fail($"{SigningOptions.SectionName}: indique solo uno entre CertificatePath y CertificateBase64.");
        }

        return ValidateOptionsResult.Success;
    }
}
