using Domain;
using Infrastructure;
using MediatR;

namespace Application.Commands;

public class SearchCommandHandler : IRequestHandler<SearchCommand, Dictionary<string, double>>
{
    private readonly DeepMorphyService _morph;
    public SearchCommandHandler(DeepMorphyService morph)
    {
        _morph = morph;
    }

    public async Task<Dictionary<string, double>> Handle(SearchCommand command, CancellationToken cancellationToken)
    {
        IEnumerable<string>? words = DeepMorphyService.GetWords(command.request);

        IEnumerable<string>? lemmas = _morph.GetLemmas(words);

        Dictionary<string, Dictionary<string, LemmaInfo>> docs = new();

        foreach (Document doc in command.InvertedIndex.Documents)
        {
            if(docs.Count == command.documentCount) break;

            foreach (string lemma in lemmas)
            {
                LemmaInfo? lemmaInfo = doc.LemmaInfoList.ContainsKey(lemma) ?
                    doc.LemmaInfoList[lemma] : null;

                if (lemmaInfo is null) continue;

                if (docs.ContainsKey(doc.DocumetID)) docs[doc.DocumetID].Add(lemma, lemmaInfo);
                else docs.Add(doc.DocumetID, new() { [lemma] = lemmaInfo });
            }
        }

        Dictionary<string, double> moreRelevant = new();
        Dictionary<string, double> lessRelevant = new();

        foreach (var doc in docs)
        {
            bool flag = lemmas.Where(lemma => doc.Value.Keys.Contains(lemma))
                .Count() == lemmas.Count() ? true : false;

            double weight = doc.Value.Values.Sum(lemmaInfo => lemmaInfo.TF_IDF);

            if (flag) moreRelevant.Add(doc.Key, weight);
            else lessRelevant.Add(doc.Key, weight);
        }

        moreRelevant = moreRelevant.OrderByDescending(weight => weight.Value)
            .ToDictionary(mr => mr.Key, mr => mr.Value);
        lessRelevant = lessRelevant.OrderByDescending(weight => weight.Value)
            .ToDictionary(lr => lr.Key, lr => lr.Value);

        if(moreRelevant.Count() is 0)
        {
            return lessRelevant;
        }

        return moreRelevant;
    }
}