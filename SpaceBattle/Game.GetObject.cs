namespace SpaceBattle;

public class GetObject : IStrategy
{
    public object Strategy(params object[] param)
    {
        var gameItemId = (string)param[1];
        var objects = (Dictionary<string, object>)param[0];
        return objects[gameItemId];
    }
}