namespace SpaceBattle;

public class ThrowFromQueue : IStrategy
{
    public object Strategy(params object[] param)
    {
        var commandQueue = (Queue<_ICommand.ICommand>)param[0];
        return commandQueue.Dequeue();
    }
}
