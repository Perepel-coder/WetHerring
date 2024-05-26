using Domain;
using MediatR;

namespace Application.Commands;

public record SearchCommand(int documentCount, string request, InvertedIndex InvertedIndex) : IRequest<Dictionary<string, double>>;
