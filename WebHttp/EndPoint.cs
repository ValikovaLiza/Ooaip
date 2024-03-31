using Hwdtech;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpaceBattle.Test")]
namespace SpaceBattle.EndPoint;

internal class EndPoint : IWebApi
{
    public Order OrderBodyEcho(Order param)
    {
        var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(param));
        message["params"] = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(param.others));

        IoC.Resolve<BattleSpace.Lib.ICommand>("Send Message", message["game id"], message).Execute();

        return param;
    }
}