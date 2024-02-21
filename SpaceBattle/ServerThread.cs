using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;

public class ServerThread
{
    private readonly BlockingCollection<_ICommand.ICommand> _queue;
    private readonly Thread _thread;
    private bool _stop = false;
    private Action _behavior;

    public ServerThread()
    {
        _queue = new BlockingCollection<_ICommand.ICommand>();
        _behavior = () =>
        {
            var cmd = _queue.Take();
            try
            {
                cmd.Execute();
            }
            catch (Exception e)
            {
                IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", cmd, e);
            }
        };
        _thread = new Thread(() =>
        {
            while (!_stop)
            {
                _behavior();
            }
        });
    }

    public void Execute()
    {
        _thread.Start();
    }
    internal void Stop()
    {
        _stop = true;
    }
    internal void UpdateBehavior(Action newBehavior)
    {
        _behavior = newBehavior;
    }
}
