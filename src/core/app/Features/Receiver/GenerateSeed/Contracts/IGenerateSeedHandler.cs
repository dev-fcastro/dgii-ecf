namespace DgiiEcf.Application.Features.Receiver.GenerateSeed.Contracts;

public interface IGenerateSeedHandler
{
    string Handle(GenerateSeedCommand command);
}
