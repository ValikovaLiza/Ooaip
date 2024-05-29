namespace SpaceBattle;

public class SetFuelStrategy : IStrategy
{
    public object Strategy(params object[] args)
    {
        var patient = (IUObject)args[0];
        return new SetFuel(patient);
    }
}