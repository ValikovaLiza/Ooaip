namespace SpaceBattle;

public class GamePushToQueue : _ICommand.ICommand
{
    Queue<_ICommand.ICommand> commandQueue;
    _ICommand.ICommand command;
    public GamePushToQueue(Queue<_ICommand.ICommand> commandQueue, _ICommand.ICommand command)
    {
        this.commandQueue = commandQueue;
        this.command = command;
    }
    public void Execute()
    {
        commandQueue.Enqueue(command);
    }
}