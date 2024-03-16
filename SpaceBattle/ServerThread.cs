using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle;

public class ServerThread
{
    private readonly BlockingCollection<_ICommand.ICommand> _queue;
    private readonly Thread _thread;
    private bool _stop = false;
    private Action _behavior;

    public ServerThread(BlockingCollection<_ICommand.ICommand> queue)
    {
        _queue = queue;
        _behavior = () =>
        {
            var cmd = _queue.Take();
            try
            {
                cmd.Execute();
            }
            catch (Exception e)
            {
                IoC.Resolve<_ICommand.ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
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
    public void UpdateBehavior(Action newBehavior)
    {
        _behavior = newBehavior;
    }
    public override bool Equals(object? obj)
    {

        if (obj == null)
        {
            return false;
        }

        if (obj.GetType() == _thread.GetType())
        {
            return _thread == (Thread)obj;
        }

        if (GetType() != obj.GetType())
        {
            return false;
        }

        throw new NotImplementedException();
    }
    public override int GetHashCode()
    {
        return _thread.GetHashCode();
    }
}
