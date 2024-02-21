using Hwdtech;

namespace SpaceBattle;
public class HardStop : _ICommand.ICommand
{
    public ServerThread thread;
    public HardStop(ServerThread thread)
    {
        this.thread = thread;
    }
    public void Execute()
    {
        var id = IoC.Resolve<int>("Get id", thread);
        var hard_s = IoC.Resolve<_ICommand.ICommand>("ServerTheard.HardStop", id);
        IoC.Resolve<_ICommand.ICommand>("ServerTheard.Command.Send", id, hard_s).Execute();
    }
}
