namespace Domain;

public class InvertedIndex
{
    public string StorageFile {  get; set; }
    public List<Document> Documents { get; set; }

    public int DocumentCount
    {
        get => Documents.Count;
    }

    public InvertedIndex()
    {
        Documents = new List<Document>();
    }
}
