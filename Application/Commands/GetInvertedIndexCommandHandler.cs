using Domain;
using Infrastructure;
using MediatR;

namespace Application.Commands;

public class GetInvertedIndexCommandHandler : IRequestHandler<GetInvertedIndexCommand, InvertedIndex?>
{
    public GetInvertedIndexCommandHandler() { }

    public async Task<InvertedIndex?> Handle(GetInvertedIndexCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return command.invertedIndexFile.ReadGz<InvertedIndex>();
        }
        catch
        {
            return null;
        }
    }
}
