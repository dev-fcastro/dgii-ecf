namespace DgiiEcf.Domain.Common.Environment.DgiiEnvironment;

/// <summary>
/// DGII electronic invoicing environments.
/// </summary>
public enum DgiiEnvironment
{
    /// <summary>Pre-certification environment (TesteCF).</summary>
    Test = 0,

    /// <summary>Certification environment (CerteCF).</summary>
    Certification = 1,

    /// <summary>Production environment (eCF).</summary>
    Production = 2,
}
