using Infrastructure;
using MediatR;

namespace Application.Commands;

public class SetFileStorageCommandHandler : IRequestHandler<SetFileStorageCommand, KeyValuePair<string, IEnumerable<string>>?>
{
    public SetFileStorageCommandHandler()
    {

    }

    public async Task<KeyValuePair<string, IEnumerable<string>>?> Handle(SetFileStorageCommand command, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                Console.Write("Введите путь к хранилищу: ");

                string? path = Console.ReadLine();

                if (path == command.returnKey.ToString())
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(path)) throw new Exception();

                return path.GetListFilesFromFolder();
            }
            catch
            {
                Console.WriteLine($"\nНедопустимый путь, попробуй снова.\n" +
                        $"Или введите команду: {command.returnKey} для возвращения в основное меню.\n");
            }
        }
    }
}