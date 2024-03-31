using System.Collections;
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

        var threadHashtable = new Hashtable();
        IoC.Resolve<ICommand>("IoC.Register", "Get HashTable", (object[] args) => threadHashtable).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
        "Add Thread To HT And Get Uuid by it",
            (object[] args) =>
            {
                var threadHashtable = IoC.Resolve<Hashtable>("Get HashTable");
                var uniqueId = Guid.NewGuid();
                threadHashtable.Add(uniqueId, (ServerThread)args[0]);
                return (object)uniqueId;
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var tab = IoC.Resolve<Hashtable>("Get HashTable");
                        var st = (ServerThread)tab[(Guid)args[0]]!;
                        st?.Execute();
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
                        var tab = IoC.Resolve<Hashtable>("Get HashTable");
                        var st = (ServerThread)tab[(Guid)args[0]]!;
                        var qu = st.GetQueue();
                        qu.Add((_ICommand.ICommand)args[1]);
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
                        var tab = IoC.Resolve<Hashtable>("Get HashTable");
                        var st = (ServerThread)tab[(Guid)args[0]]!;
                        new HardStop(st).Execute();
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
                        var tab = IoC.Resolve<Hashtable>("Get HashTable");
                        var st = (ServerThread)tab[(Guid)args[0]]!;
                        new SoftStop(st, (Action)args[1]).Execute();
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
        var uuid = IoC.Resolve<Guid>("Add Thread To HT And Get Uuid by it", st);

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute());
        var threadStoped = false;

        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", uuid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, hs).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, command.Object).Execute();

        mre.WaitOne(1000);
        Assert.Single(q);
        Assert.True(threadStoped);
    }

    [Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To HT And Get Uuid by it", st);

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);
        var threadStoped = false;

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", uuid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute());

        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, command.Object).Execute();

        mre.WaitOne(1000);
        Assert.Empty(q);
        Assert.True(threadStoped);
    }

    [Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To HT And Get Uuid by it", st);
        var st2 = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid2 = IoC.Resolve<Guid>("Add Thread To HT And Get Uuid by it", st2);

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var threadStoped = false;

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", uuid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        var ecommand = new Mock<_ICommand.ICommand>();
        ecommand.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid, ecommand.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid2, ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Empty(q);
        Assert.True(threadStoped);
        Assert.Throws<Exception>(() => ss.Execute());
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To HT And Get Uuid by it", st);
        var st2 = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid2 = IoC.Resolve<Guid>("Add Thread To HT And Get Uuid by it", st2);

        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", uuid).Execute();

        var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", uuid, () =>{ });

        IoC.Resolve<_ICommand.ICommand>("Send Command", uuid2, hs).Execute();

        Assert.Throws<Exception>(() => hs.Execute());
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
