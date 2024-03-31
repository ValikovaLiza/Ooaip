using System.Collections.Generic;
using System.Runtime.Serialization;
using CoreWCF.OpenApi.Attributes;

namespace SpaceBattle.EndPoint;
[DataContract(Name = "Message")]
public class Message
{
    [DataMemder(Description = "type", Message = 1)]
    public string? type { get; set; }
    [DataMemder(Description = "game id", Message = 2)]
    public string? game_id { get; set; }
    [DataMemder(Description = "game item", Message = 3)]
    public string? game_item { get; set; }
    [DataMemder(Description = "initial velocity", Message = 4)]
    public string? init_velocity { get; set; }
}

[DataContract(Name = "Parameters")]
public class Parameters
{
    [DataMember(Name = "param")]
    public string? param { get; set; }
    [DataMember(Name = "value")]
    public object? val { get; set; }
}
