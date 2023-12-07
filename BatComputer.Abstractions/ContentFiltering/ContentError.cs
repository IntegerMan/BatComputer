namespace MattEland.BatComputer.Abstractions.ContentFiltering;

public class ContentError
{
    public string message { get; set; }
    public string type { get; set; }
    public string param { get; set; }
    public string code { get; set; }
    public int status { get; set; }
    public ContentInnerError innererror { get; set; }
}