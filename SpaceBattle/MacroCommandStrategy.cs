using Hwdtech;

namespace SpaceBattle;

public class MacroCommandStrategy : IStrategy
{
    public object Strategy(params object[] args)
    {
        var key = (string)args[0];
        var obj = (IUObject)args[1];

        var dependencies = IoC.Resolve<IList<string>>("SpaceBattle.Operation." + key);
        IList<_ICommand.ICommand> list = new List<_ICommand.ICommand>();

        foreach (var d in dependencies)
        {
            list.Add(IoC.Resolve<_ICommand.ICommand>(d, obj));
        }

        return new MacroCommand(list);
    }
}
