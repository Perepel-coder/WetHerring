using MediatR;

namespace Application.Commands;

public record GetFileStorageCommand(string fileStorageInfoPath) : IRequest<KeyValuePair<string, IEnumerable<string>>>;