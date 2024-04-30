using Hwdtech;
using Hwdtech.Ioc;

namespace SpaceBattle;
public class CreateNewStrategy : IStrategy
{
    public object Strategy(params object[] args)
    {
        var gameId = (string)args[0];

        new InitScopeBasedIoCImplementationCommand().Execute();
        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));

        var commandQueue = new Queue<_ICommand.ICommand>();
        var receiver = new QueueAdapter(commandQueue);

        var gamesDictionary = IoC.Resolve<Dictionary<string, Queue<_ICommand.ICommand>>>("Game.Get.GamesDictioanary");
        gamesDictionary.Add(gameId, commandQueue);

        return new GameCommand(scope, receiver);
    }
}