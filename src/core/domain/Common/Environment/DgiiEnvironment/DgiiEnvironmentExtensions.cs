namespace DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

public static class DgiiEnvironmentExtensions
{
    /// <summary>
    /// Path segment used by the DGII services: <c>TesteCF</c>, <c>CerteCF</c> or <c>eCF</c>.
    /// </summary>
    public static string ToPathSegment(this DgiiEnvironment environment) => environment switch
    {
        DgiiEnvironment.Test => "TesteCF",
        DgiiEnvironment.Certification => "CerteCF",
        DgiiEnvironment.Production => "eCF",
        _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null),
    };

    /// <summary>
    /// Code used by the service status API (<c>ambiente</c>): 1 test, 2 production, 3 certification.
    /// </summary>
    public static int ToStatusCode(this DgiiEnvironment environment) => environment switch
    {
        DgiiEnvironment.Test => 1,
        DgiiEnvironment.Production => 2,
        DgiiEnvironment.Certification => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null),
    };
}
