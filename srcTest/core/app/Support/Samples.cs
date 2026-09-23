namespace DgiiEcf.Application.Tests.Support;

internal static class Samples
{
    public static string Read(string fileName) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Samples", fileName));

    /// <summary>
    /// Sample files keep the line endings they were committed with; compare them as LF without trailing whitespace.
    /// </summary>
    public static string Normalize(string text) => text.TrimStart('﻿').Replace("\r\n", "\n").TrimEnd();
}
