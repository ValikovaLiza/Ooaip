using Hwdtech;
namespace SpaceBattle;

public class StartMoveCommand : ICommand
{
    private readonly IMoveCommandStartable _obj;

    public StartMoveCommand(IMoveCommandStartable obj)
    {
        _obj = obj;
    }

    public void Execute()
    {
        _obj.Dict.ToList().ForEach(o => IoC.Resolve<ICommand>("Target.Register", _obj.UObject, o.Key, o.Value).Execute());
        var mCommand = IoC.Resolve<ICommand>("Moving.Commands", _obj.UObject);
        var injectable = IoC.Resolve<ICommand>("Commands.Injectable", mCommand);
        IoC.Resolve<ICommand>("Target.Register", _obj.UObject, "Moving.Commands", mCommand).Execute();
        IoC.Resolve<IQueue>("Queue.Push", injectable).Add(mCommand);
    }
}
