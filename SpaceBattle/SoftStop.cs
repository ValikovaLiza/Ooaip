using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;
public class SoftStop : _ICommand.ICommand
{
    public ServerThread thread;

    public SoftStop(ServerThread thread)
    {
        this.thread = thread;
    }
    public void Execute()
    {
        var queue = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");
        if (thread.Equals(Thread.CurrentThread))
        {
            thread.UpdateBehavior(() =>
            {
                while (queue.TryTake(out var cmd))
                {
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception e)
                    {
                        IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                    }
                }

                thread.Stop();
            });
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
