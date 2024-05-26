using Domain;
using MediatR;

namespace Application.Commands;

public record class GetInvertedIndexCommand(string invertedIndexFile) : IRequest<InvertedIndex?>;
