namespace MattEland.BatComputer.Plugins.Sessionize.Model;
public class SessionGroup
{
    public string GroupId { get; set; } // TODO: Probably a GUID, but I don't have a value in reference data so string is safer
    public string GroupName { get; set; }
    public List<Session> Sessions { get; set; } = new();
}
