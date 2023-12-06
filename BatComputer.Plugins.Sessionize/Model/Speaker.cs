namespace MattEland.BatComputer.Plugins.Sessionize.Model;

public class Speaker
{
    public string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string FullName { get; set; }
    public required string Bio { get; set; }
    public required string TagLine { get; set; }
    public required string ProfilePicture { get; set; }
    public List<SpeakerSession> Sessions { get; set; } = new();
    //public bool IsTopSpeaker { get; set; }
    //public List<string> Links { get; set; }
    //public List<string> Categories { get; set; }
}