﻿using System.Collections.Concurrent;
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

        var idDict = new ConcurrentDictionary<int, ServerThread>();
        var queueDict = new ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>();

        IoC.Resolve<ICommand>("IoC.Register", "Server.Dict", (object[] args) => idDict).Execute();
        IoC.Resolve<ICommand>("IoC.Register", "Server.QueueDict", (object[] args) => queueDict).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        idDict.TryAdd((int)args[0], (ServerThread)args[1]);
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        thread.Execute();
                        if (args.Length == 3 && args[2] != null)
                        {
                            new ActionCommand((Action)args[2]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Add Command To QueueDict",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        queueDict.TryAdd((int)args[0], (BlockingCollection<_ICommand.ICommand>)args[1]);
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
                        var qu = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[(int)args[0]];
                        qu.Add((_ICommand.ICommand)args[1]);
                        if (args.Length == 3 && args[2] != null)
                        {
                            new ActionCommand((Action)args[2]).Execute();
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
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        new HardStop(thread).Execute();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
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
                        var thread = IoC.Resolve<ConcurrentDictionary<int, ServerThread>>("Server.Dict")[(int)args[0]];
                        var qu = IoC.Resolve<ConcurrentDictionary<int, BlockingCollection<_ICommand.ICommand>>>("Server.QueueDict")[(int)args[0]];
                        new SoftStop(thread, (Action)args[1], qu).Execute();
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

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 1, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 1, st).Execute();

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 1, () => { mre.Set(); });

        IoC.Resolve<_ICommand.ICommand>("Send Command", 1, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 1, hs).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 1, command.Object).Execute();

        mre.WaitOne(1000);

        Assert.Single(q);
        command.Verify(m => m.Execute(), Times.Once);
    }

    [Fact]
    public void SoftStopShouldStopServerThreadAndHaveCommandWithException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();
        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 2, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 2, st).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 2, () => { mre.Set(); }, q);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        var ecommand = new Mock<_ICommand.ICommand>();
        ecommand.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => ss.Execute());
        Assert.Empty(q);
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();
        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 3, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 3, st).Execute();

        var ecommand = new Mock<_ICommand.ICommand>();
        ecommand.Setup(m => m.Execute()).Throws(new Exception());

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 3, () => { mre.Set(); });

        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, hs).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 3, ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Single(q);
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 4, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 4, st).Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 4, () => { mre.Set(); });

        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, hs).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => hs.Execute());
        Assert.Empty(q);
    }

    [Fact]
    public void SoftStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 5, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 5, st).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 5, () => { mre.Set(); }, q);

        IoC.Resolve<_ICommand.ICommand>("Send Command", 5, ss).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => ss.Execute());
        Assert.Empty(q);
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
    public void NegativeEqualThreads()
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
    public void PositiveCurrentEqualThreads()
    {
        var q = new BlockingCollection<_ICommand.ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var nothing = 22;

        Assert.False(st1.Equals(nothing));
    }
}
