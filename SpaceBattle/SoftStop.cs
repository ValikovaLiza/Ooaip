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
            if (queue.Count > 0)
            {
                var cmd = queue.Take();
                try
                {
                    cmd.Execute();
                }
                catch (Exception e)
                {
                    IoC.Resolve<ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                }
            }
            else
            {
                IoC.Resolve<ICommand>("Server.Commands.HardStop", thread, action).Execute();
            }
        });
    }
}

