using Domain;
using MediatR;

namespace Application.Commands;

public record CalculateInvertedIndexCommand(
    KeyValuePair<string, IEnumerable<string>> fileStorage,
    string invertedIndexFile,
    string? particlesFile = null) : IRequest<InvertedIndex>;
