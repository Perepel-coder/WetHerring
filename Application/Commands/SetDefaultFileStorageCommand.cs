using MediatR;

namespace Application.Commands;

public record SetDefaultFileStorageCommand(string fileStorageInfoPath, string fileStorage) : IRequest<bool>;
