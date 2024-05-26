using Infrastructure;
using MediatR;
using System.IO;

namespace Application.Commands;

public class SetDefaultFileStorageCommandHandler : IRequestHandler<SetDefaultFileStorageCommand, bool>
{
    public SetDefaultFileStorageCommandHandler()
    {

    }

    public async Task<bool> Handle(SetDefaultFileStorageCommand command, CancellationToken cancellationToken)
    {
        Console.WriteLine("Назначить текущее хранилище стандартным?\nДа - 1\nНет - 0\nДругое - выход в меню");

        try
        {
            Console.Write("\nВаша команда: ");
            int key = Convert.ToInt32(Console.ReadLine());

            if (key is 1)
            {
                FileStream stream = new(command.fileStorageInfoPath, FileMode.Open);
                IEnumerable<int>? data = command.fileStorage.ToTypeInt();
                stream.SaveFile_TXT_DOCX(data);
                stream.Close();

                return true;
            }

            throw new Exception();
        }
        catch
        {
            return false;
        }
    }
}
