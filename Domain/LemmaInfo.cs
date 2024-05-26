namespace Domain;

public class LemmaInfo
{
    public double TF { get; set; }
    public double IDF { get; set; }

    public double TF_IDF
    {
        get => TF * IDF;
    }
}