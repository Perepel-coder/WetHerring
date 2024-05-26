using Domain;
using Infrastructure;
using MediatR;
using System.IO;

namespace Application.Commands;

public class CalculateInvertedIndexCommandHandler : IRequestHandler<CalculateInvertedIndexCommand, InvertedIndex>
{
    private readonly DeepMorphyService _morph;
    public CalculateInvertedIndexCommandHandler(DeepMorphyService morph)
    {
        _morph = morph;
    }

    public async Task<InvertedIndex> Handle(CalculateInvertedIndexCommand command, CancellationToken cancellationToken)
    {
        List<string> particles = new();
        Dictionary<string, IEnumerable<string>> documents = new();
        if (command.particlesFile is not null)
        {
            FileStream stream = new(command.particlesFile, FileMode.Open);
            particles = stream.OpenFile_TXT_DOCX().ToTypeString().Split().ToList();
            stream.Close();
        }


        foreach (string file in command.fileStorage.Value)
        {
            FileStream stream = new(file, FileMode.Open);
            IEnumerable<byte> bytes = stream.OpenFile_TXT_DOCX();
            var text = bytes.ToTypeString();
            stream.Close();

            IEnumerable<string>? words = DeepMorphyService.GetWords(text);

            documents.Add(stream.Name, words);

            Console.WriteLine($"Получен документ: {stream.Name}");
        }

        _morph.AnalyzeDocuments(command.fileStorage.Key, documents, particles);

        command.invertedIndexFile.SaveGz(_morph.Index);

        Console.WriteLine("Индексная структура успешно сгенерированна и сохранена в файл:\n");
        Console.WriteLine($"{command.invertedIndexFile}\n");

        return _morph.Index;
    }
}
