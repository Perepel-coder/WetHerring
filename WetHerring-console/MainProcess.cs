using Application.Commands;
using Domain;
using MediatR;

namespace WetHerring_console;

public class MainProcess
{
    enum KEY
    {
        MENU,
        INFO,
        SET_FILE_STORAGE,
        SET_DEFAULT_FILE_STORAGE,
        CALC_INVERTED_INDEX,
        SEARCH,
        EXIT,
        DEFAULT
    }
    private readonly ISender _sender;
    private readonly int _documentCount = 20;
    private readonly string _programName = "«Мокрая Селедка»";
    private readonly string _particlesFile = @$"{Directory.GetCurrentDirectory()}\Resourses\particles.txt";
    private readonly string _invertedIndexFile = @$"{Directory.GetCurrentDirectory()}\Resourses\invertedIndex";
    private readonly string _fileStorageInfoPath = @$"{Directory.GetCurrentDirectory()}\Resourses\fileStorageInfo.txt";

    private KeyValuePair<string, IEnumerable<string>> _currentFileStorage;
    private InvertedIndex? _invertedIndex;

    public MainProcess(ISender sender)
    {
        _sender = sender;
        GetGetFileStorage();
        GetInvertedIndexFromFile();
    }

    private async void GetGetFileStorage()
    {
        GetFileStorageCommand command = new(_fileStorageInfoPath);
        _currentFileStorage = await _sender.Send(command);
    }

    private async void GetInvertedIndexFromFile()
    {
        GetInvertedIndexCommand command = new(_invertedIndexFile);

        _invertedIndex = await _sender.Send(command);
    }

    private KEY UserInput()
    {
        try
        {
            Console.Write("\nВаша команда: ");
            return (KEY)Convert.ToInt32(Console.ReadLine());
        }
        catch
        {
            return KEY.DEFAULT;
        }
    }

    public async void Run()
    {
        KEY command = KEY.MENU;

        Console.WriteLine($"\n{_programName}: простейшей модель инвертированного индекса\n");

        while (true)
        {
            switch (command)
            {
                case KEY.MENU:

                    Console.WriteLine("Нажмите 1 - информация о программе.");
                    Console.WriteLine("Нажмите 2 - установить новую директорию.");
                    Console.WriteLine("Нажмите 3 - назначить текущую директорию дефолтной.");
                    Console.WriteLine("Нажмите 4 - рассчитать индексную структуру.");
                    Console.WriteLine("Нажмите 5 - поиск по директории.");
                    Console.WriteLine("Нажмите 6 - выход.");

                    command = UserInput();
                    break;
                case KEY.INFO:

                    Console.WriteLine($"\n{_programName} очень проста в использовании!\n");

                    Console.WriteLine($"Стандартное хранилище файлов: {_currentFileStorage.Key}");
                    Console.WriteLine($"Но вы можете установить собственное хранилище. " +
                        $"Для этого введите команду: {(int)KEY.SET_FILE_STORAGE}");
                    Console.WriteLine($"Чтобы назначить текущее хранилище стандартным, " +
                        $"введите команду: {(int)KEY.SET_DEFAULT_FILE_STORAGE}\n");

                    Console.WriteLine($"Индексная структура находится в хранилище. " +
                        $"{_programName} ожидает найти файл: {_invertedIndexFile}.");
                    Console.WriteLine($"Менять название файла строго запрещено!");
                    Console.WriteLine($"Вы можете сгенерировать индексную структуру. " +
                        $"Для этого введите команду: {(int)KEY.CALC_INVERTED_INDEX}\n");

                    Console.WriteLine($"Поиск по хранилищу осуществляется только при наличии индексной структуры.");
                    Console.WriteLine($"Для того, чтобы запустить поисковик введите команду: {(int)KEY.SEARCH}\n");

                    command = UserInput();
                    break;
                case KEY.SET_FILE_STORAGE:

                    Console.WriteLine($"Текущее хранилище: {_currentFileStorage.Key}\n");

                    SetFileStorageCommand sfsCommand = new((int)KEY.MENU, _currentFileStorage.Key);

                    KeyValuePair<string, IEnumerable<string>>? sfsResult = await _sender.Send(sfsCommand);

                    if (sfsResult is null) { command = KEY.MENU; break; }

                    _currentFileStorage = (KeyValuePair<string, IEnumerable<string>>)sfsResult;

                    Console.WriteLine("Новое хранилище успешно установлено");

                    command = UserInput();
                    break;
                case KEY.SET_DEFAULT_FILE_STORAGE:
                    Console.WriteLine($"Текущее хранилище: {_currentFileStorage.Key}\n");

                    SetDefaultFileStorageCommand sdfsCommand = new(_fileStorageInfoPath, _currentFileStorage.Key);

                    bool sdfsResult = await _sender.Send(sdfsCommand);

                    if (sdfsResult is false) { command = KEY.MENU; break; }

                    Console.WriteLine("Новое хранилище успешно установлено как дефолтное");

                    command = UserInput();

                    break;
                case KEY.CALC_INVERTED_INDEX:
                    Console.WriteLine($"Текущее хранилище: {_currentFileStorage.Key}\n");

                    CalculateInvertedIndexCommand ciiCommand = new(_currentFileStorage, _invertedIndexFile, _particlesFile);

                    _invertedIndex = await _sender.Send(ciiCommand);

                    command = UserInput();
                    break;
                case KEY.SEARCH:
                    Console.WriteLine($"Текущее хранилище: {_currentFileStorage.Key}\n");

                    if (_invertedIndex is null && _invertedIndex?.StorageFile != _currentFileStorage.Key)
                    {
                        Console.WriteLine($"Индексная структура для хранилища: {_currentFileStorage.Key} не найдена!");
                        Console.WriteLine($"Вы можете сгенерировать индексную структуру. " +
                        $"Для этого введите команду: {(int)KEY.CALC_INVERTED_INDEX}\n");
                    }
                    else
                    {
                        Console.WriteLine($"Найдена индексная структура для хранилища:\n" +
                            $"{_invertedIndex.StorageFile}\n");

                        Console.Write("Ваш запрос: ");
                        string? request = Console.ReadLine();

                        if (string.IsNullOrEmpty(request))
                        {
                            Console.WriteLine("\nНевозможно отправить пустой запрос");
                            command = KEY.SEARCH;
                            break;
                        }

                        var date_start = DateTime.Now;

                        SearchCommand scCommand = new(_documentCount, request, _invertedIndex);

                        Dictionary<string, double> documents = await _sender.Send(scCommand);

                        Console.WriteLine($"\nОтобрано {_documentCount} документов по степени релевантности\n");

                        foreach (var doc in documents)
                        {
                            Console.WriteLine($"Документ: {doc.Key.Split('\\').Last()} -> " +
                                $"Общий вес по запросу: {doc.Value}");
                        }

                        var date_end = DateTime.Now;

                        Console.WriteLine($"\nВремя выполнения запроса: {(date_end - date_start).Milliseconds} мс\n");

                    }
                    command = UserInput();
                    break;
                case KEY.EXIT: return;
                default:
                    Console.WriteLine("\nВыбирите команду из предложенного списка:\n");
                    command = KEY.MENU;
                    break;
            }
        }
    }
}