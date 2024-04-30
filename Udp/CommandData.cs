namespace Udp;

public class CommandData
{
    public string? CommandType { get; set; }
    public string? gameId { get; set; }
    public string? gameItemId { get; set; }
    public Dictionary<string, int>? DictOfExtra { get; set; }
}
