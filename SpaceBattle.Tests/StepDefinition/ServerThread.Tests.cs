using System.Collections.Concurrent;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
namespace SpaceBattle.Test;
public class ServerTheardTests
{
    public ServerTheardTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var q = new BlockingCollection<_ICommand.ICommand>(10);
                        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
                        IoC.Resolve<ICommand>("IoC.Register", "Get BlockingQueue", (object[] args) => q).Execute();
                        IoC.Resolve<ICommand>("IoC.Register", "Get ServerThread", (object[] args) => st).Execute();
                        st.Execute();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Send Command",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var qu = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");
                        qu.Add((_ICommand.ICommand)args[0]);
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Hard Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var thread = IoC.Resolve<ServerThread>("Get ServerThread");
                        new HardStop(thread).Execute();
                        if (args.Length == 1 && args[0] != null)
                        {
                            new ActionCommand((Action)args[0]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Soft Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var thread = IoC.Resolve<ServerThread>("Get ServerThread");
                        new SoftStop(thread).Execute();
                    }
                );
            }
        ).Execute();
    }

    [Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread").Execute();

        var executedOnce = false;
        var mre = new ManualResetEvent(false);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Callback(() =>
        {
            if (!executedOnce)
            {
                executedOnce = true;
                mre.Set();
            }
        });

        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread");

        IoC.Resolve<_ICommand.ICommand>("Send Command", command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", hs).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", command.Object).Execute();

        mre.WaitOne(1000);
        var queue = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");
        Assert.Single(queue);
    }

    [Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread").Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread");

        var command = new Mock<_ICommand.ICommand>();
        var executeActions = new Action[]
        {
            () => {},
            () => mre.Set()
        };

        var executionStep = 0;

        command.Setup(m => m.Execute()).Callback(() =>
        {
            executeActions[executionStep]();
            executionStep++;
        });

        IoC.Resolve<_ICommand.ICommand>("Send Command", command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", command.Object).Execute();

        mre.WaitOne(1000);
        var queue = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");
        Assert.Empty(queue);
    }

    [Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread").Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", () => { mre.Set(); });

        var ecommand = new Mock<_ICommand.ICommand>();
        var executeActions = new Action[]
        {
            () => {},
            () => mre.Set()
        };

        var executionStep = 0;

        ecommand.Setup(m => m.Execute()).Callback(() =>
        {
            executeActions[executionStep]();
            executionStep++;
        }).Throws(new Exception());

        IoC.Resolve<_ICommand.ICommand>("Send Command", ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => ss.Execute());
        var queue = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");
        Assert.Empty(queue);
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread").Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", () => { mre.Set(); });

        IoC.Resolve<_ICommand.ICommand>("Send Command", hs).Execute();

        mre.WaitOne(1000);
        var queue = IoC.Resolve<BlockingCollection<_ICommand.ICommand>>("Get BlockingQueue");

        Assert.Throws<Exception>(() => hs.Execute());
        Assert.Empty(queue);
    }

    [Fact]
    public void HashCodeTheSame()
    {
        var queue1 = new BlockingCollection<_ICommand.ICommand>();
        var serverThread1 = new ServerThread(queue1, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var queue2 = new BlockingCollection<_ICommand.ICommand>();
        var serverThread2 = new ServerThread(queue2, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Assert.True(serverThread1.GetHashCode() != serverThread2.GetHashCode());
    }

    [Fact]
    public void EqualThreadsWithNull()
    {
        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Assert.False(st.Equals(null));
    }

    [Fact]
    public void PositiveEqualThreads()
    {
        var q1 = new BlockingCollection<_ICommand.ICommand>(10);

        var st1 = new ServerThread(q1, Thread.CurrentThread);
        var st2 = new ServerThread(q1, Thread.CurrentThread);

        Assert.False(st1.Equals(st2));
    }

    [Fact]
    public void AbsoluteDifferendEquals()
    {
        var q = new BlockingCollection<_ICommand.ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var nothing = 22;

        Assert.False(st1.Equals(nothing));
    }
}
