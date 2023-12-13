namespace SpaceBattle.Tests;

using Hwdtech;
using Hwdtech.Ioc;
using Moq;

public class ActionCommand : ICommand
{
    private readonly Action _action;
    public ActionCommand(Action action) => _action = action;
    public void Execute()
    {
        _action();
    }
}