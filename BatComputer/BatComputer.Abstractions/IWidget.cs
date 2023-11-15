namespace MattEland.BatComputer.Abstractions;

public interface IWidget
{
    void SetTitle(string? title);
    string? GetTitle();
    void UseSampleData();
}