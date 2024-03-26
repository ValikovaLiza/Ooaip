using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;
public class SoftStop : _ICommand.ICommand
{
    public ServerThread thread;
    public Action action = () => { };

    public SoftStop(ServerThread thread, Action action)
    {
        this.thread = thread;
        this.action = action;
    }
    public void Execute()
    {
        var queue = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");
        if (thread.Equals(Thread.CurrentThread))
        {
            thread.UpdateBehavior(() =>
            {
                if (queue.TryTake(out var command) == true)
                {
                    var cmd = queue.Take();
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception e)
                    {
                        IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                    }
                }
                else
                {
                    thread.Stop();
                }
            });
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
