using DeepMorphy;
using Domain;

namespace Infrastructure;

public class DeepMorphyService
{
    private readonly MorphAnalyzer _morphAnalyzer;
    private const int _groupSize = 100;

    public InvertedIndex Index { get; private set; }

    public DeepMorphyService(
        bool withLemmatization = false,
        bool useEnGrams = false,
        bool withTrimAndLower = true,
        int maxBatchSize = 4096)
    {
        _morphAnalyzer = new(
            withLemmatization: withLemmatization,
            useEnGrams: useEnGrams,
            withTrimAndLower: withTrimAndLower,
            maxBatchSize: maxBatchSize);

        Index = new();
    }


    private void _ParseGroup(Dictionary<string, int> group, Document document)
    {
        var tags = _morphAnalyzer
                .Parse(group.Keys)
                .Select(info => info.BestTag).ToList();

        for (int i = 0; i < tags.Count; i++)
        {
            if (tags[i].HasLemma) Step(tags[i].Lemma, group.ElementAt(i).Value);
            else Step(group.ElementAt(i).Key, group.ElementAt(i).Value);
        }

        void Step(string key, double value)
        {
            if (document.LemmaInfoList.ContainsKey(key))
            {
                document.LemmaInfoList[key].TF += value;
            }
            else
            {
                document.LemmaInfoList.Add(key, new LemmaInfo { TF = value });
            }
        }
    }



    public static IEnumerable<string> GetWords(string text)
    {
        char[] chars = text.ToLower().ToCharArray();

        List<char> pureCharacters = new();

        foreach (var _char in chars)
        {
            if (char.IsLetterOrDigit(_char) || char.IsWhiteSpace(_char) || _char == '-')
            {
                pureCharacters.Add(char.IsWhiteSpace(_char) ? ' ' : _char);
            }
        }

        var words = string.Join("", pureCharacters)
            .Split()
            .Where(word =>
            !string.IsNullOrEmpty(word) &&
            word.Length > 2);

        return words;
    }

    public Dictionary<string, int> CountLemmas(IEnumerable<string> lemmas)
    {
        Dictionary<string, int> counterLemmas = new();

        foreach (var lemma in lemmas)
        {
            if (counterLemmas.ContainsKey(lemma)) counterLemmas[lemma]++;
            else counterLemmas.Add(lemma, 1);
        }

        return counterLemmas;
    }

    public void CalculateIDF()
    {
        foreach (Document document in Index.Documents)
        {
            foreach (var lemma in document.LemmaInfoList)
            {
                int terminContains = Index.Documents
                    .Select(doc => doc.LemmaInfoList.ContainsKey(lemma.Key))
                    .Count();

                double idf = Math.Log((Index.DocumentCount * 1.0f) / terminContains);

                lemma.Value.IDF = idf is 0 ? 0.5 : idf;
            }
        }

        Console.WriteLine("Расчитали IDF");
    }

    public IEnumerable<string> GetLemmas(IEnumerable<string> words)
    {
        List<string> lemmas = new();

        var tags = _morphAnalyzer
                .Parse(words)
                .Select(info => info.BestTag).ToList();

        for (int i = 0; i < tags.Count; i++)
        {
            if (tags[i].HasLemma) lemmas.Add(tags[i].Lemma);
            else lemmas.Add(words.ElementAt(i));
        }

        return lemmas;
    }

    private void AnalyzeDocument(
        string documentName,
        IEnumerable<string> words,
        IEnumerable<string>? particles = null)
    {
        Console.WriteLine($"\nПарсим докумен: {documentName.Split('\\').Last()}");

        Index.Documents.Add(new Document
        {
            DocumetID = documentName
        });

        Document document = Index.Documents.Last();

        words = particles is not null ? words.Where(word => !particles.Contains(word)) : words;

        Console.WriteLine($"Удалили мусор");

        Dictionary<string, int>[] groupsOfLemmas = CountLemmas(words).Chunk(_groupSize)
            .Select(g => g.ToDictionary(l => l.Key, l => l.Value)).ToArray();

        int countGroups = groupsOfLemmas.Count();
        Console.WriteLine($"Разбили на группы. Кол-во групп: {countGroups}");

        int countWords = words.Count();

        for (int i = 0; i < countGroups; i++)
        {
            _ParseGroup(groupsOfLemmas[i], document);

            Console.WriteLine($"Группа: {i + 1} | LemmasInfo: {document.LemmaInfoList.Count}");
        }

        foreach (var lemma in document.LemmaInfoList) { lemma.Value.TF /= countWords; }

        Console.WriteLine($"Закончил парсить! Кол-во лексем: {document.LemmaInfoList.Count}");
    }

    public void AnalyzeDocuments(
        string storageFile,
        Dictionary<string, IEnumerable<string>> documents,
        IEnumerable<string>? particles = null)
    {

        Index.StorageFile = storageFile;

        foreach (var document in documents)
        {
            AnalyzeDocument(document.Key, document.Value, particles);
        }

        CalculateIDF();
    }
}
