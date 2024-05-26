namespace Domain;

public class Document
{
    public string DocumetID { get; set; }
    public Dictionary<string, LemmaInfo> LemmaInfoList { get; set; }

    public Document()
    {
        DocumetID = string.Empty;
        LemmaInfoList = new();
    }
}