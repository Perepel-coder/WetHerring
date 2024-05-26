using MediatR;

namespace Application.Commands;

public record SetFileStorageCommand(int returnKey, string currentFileStorage) : IRequest<KeyValuePair<string, IEnumerable<string>>?>;
