namespace DgiiEcf.Application.Common.Responses.DgiiMessage;

/// <summary>
/// Message returned by DGII in the <c>mensajes</c> collection.
/// </summary>
public sealed record DgiiMessage(string Valor, int Codigo);
