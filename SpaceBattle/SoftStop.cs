using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;
public class SoftStop : _ICommand.ICommand
{
    public ServerThread thread;
    private readonly BlockingCollection<_ICommand.ICommand> queue;
    public Action action = () => { };

    public SoftStop(ServerThread thread, Action action, BlockingCollection<_ICommand.ICommand> queue)
    {
        this.thread = thread;
        this.action = action;
        this.queue = queue;
    }
    public void Execute()
    {
        thread.UpdateBehavior(() =>
        {
            if (queue.Count != 0)
            {
                thread.Execute();
            }
            else
            {
                var id = IoC.Resolve<int>("Get id", thread);
                IoC.Resolve<_ICommand.ICommand>("ServerTheard.Command.Send", id, IoC.Resolve<_ICommand.ICommand>("ServerTheard.HardStop", id, action)).Execute();
            }
        });
    }
}

