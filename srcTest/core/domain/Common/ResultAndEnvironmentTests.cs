using DgiiEcf.Domain.Common.Environment.DgiiEnvironment;
using DgiiEcf.Domain.Common.Results;
using DgiiEcf.Domain.Reception.ReceptionRules;

namespace DgiiEcf.Domain.Tests.Common;

public sealed class ResultAndEnvironmentTests
{
    [Fact]
    public void Failure_ValueAccess_Throws()
    {
        var result = Result<int>.Failure(new Error("x", "y"));

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_WithNone_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Theory]
    [InlineData(DgiiEnvironment.Test, "TesteCF", 1)]
    [InlineData(DgiiEnvironment.Certification, "CerteCF", 3)]
    [InlineData(DgiiEnvironment.Production, "eCF", 2)]
    public void Environment_MapsToDgiiValues(DgiiEnvironment environment, string segment, int statusCode)
    {
        Assert.Equal(segment, environment.ToPathSegment());
        Assert.Equal(statusCode, environment.ToStatusCode());
    }

    [Theory]
    [InlineData("32", true)]
    [InlineData("41", true)]
    [InlineData("43", true)]
    [InlineData("45", true)]
    [InlineData("46", true)]
    [InlineData("47", true)]
    [InlineData("31", false)]
    [InlineData("33", false)]
    [InlineData("34", false)]
    [InlineData("44", false)]
    [InlineData(null, false)]
    public void IsExcludedType_FollowsTheEmisorReceptorStandard(string? type, bool excluded)
    {
        Assert.Equal(excluded, ReceptionRules.IsExcludedType(type));
    }
}
