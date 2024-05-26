using Infrastructure;
using MediatR;
using System.IO;

namespace Application.Commands;

public class GetFileStorageCommandHandler : IRequestHandler<GetFileStorageCommand, KeyValuePair<string, IEnumerable<string>>>
{
    public GetFileStorageCommandHandler() { }

    public async Task<KeyValuePair<string, IEnumerable<string>>> Handle(GetFileStorageCommand command, CancellationToken cancellationToken)
    {
        FileStream stream = new(command.fileStorageInfoPath, FileMode.Open);
        string fileStorage = stream.OpenFile_TXT_DOCX().ToTypeString();
        stream.Close();
        return fileStorage.GetListFilesFromFolder();
    }
}
