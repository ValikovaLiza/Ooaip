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

        var idDict = new Dictionary<int, ServerThread>();
        var queueDict = new Dictionary<int, BlockingCollection<_ICommand.ICommand>>();

        IoC.Resolve<ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        idDict.Add((int)args[0], (ServerThread)args[1]);
                        var st = (ServerThread)args[1];
                        st.Execute();
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
                        queueDict.Add((int)args[0], (BlockingCollection<_ICommand.ICommand>)args[1]);
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
                        var queue = queueDict[(int)args[0]];
                        queue.Add((_ICommand.ICommand)args[1]);
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
                        new HardStop(idDict[(int)args[0]]).Execute();
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
                        new SoftStop(idDict[(int)args[0]], (Action)args[1], (BlockingCollection<_ICommand.ICommand>)args[2]).Execute();
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
        var st = new ServerThread(q);

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
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q);

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 2, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 2, st).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 2, () => { mre.Set(); }, q);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 2, command.Object).Execute();

        mre.WaitOne(1000);

        Assert.Empty(q);
    }

    // [Fact]
    // public void HardStopCanNotStopServerBecauseOfException()
    // {
    //     IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

    //     var cmd = new Mock<_ICommand.ICommand>();
    //     IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

    //     var q = new BlockingCollection<_ICommand.ICommand>(10);
    //     var st = new ServerThread(q);

    //     IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 3, q).Execute();
    //     IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 3, st).Execute();

    //     var ecommand = new Mock<_ICommand.ICommand>();
    //     ecommand.Setup(m => m.Execute()).Throws(new Exception());

    //     var mre = new ManualResetEvent(false);
    //     var hs = IoC.Resolve<_ICommand.ICommand>("Hard Stop The Thread", 3, () => { mre.Set(); });

    //     IoC.Resolve<_ICommand.ICommand>("Send Command", 3, ecommand.Object).Execute();
    //     IoC.Resolve<_ICommand.ICommand>("Send Command", 3, hs).Execute();
    //     IoC.Resolve<_ICommand.ICommand>("Send Command", 3, ecommand.Object).Execute();

    //     mre.WaitOne(1000);

    //     Assert.Single(q);
    // }

    [Fact]
    public void SoftStopCanNotStopServerBecauseOfException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<_ICommand.ICommand>();
        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q);

        IoC.Resolve<_ICommand.ICommand>("Add Command To QueueDict", 4, q).Execute();
        IoC.Resolve<_ICommand.ICommand>("Create and Start Thread", 4, st).Execute();

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        var ecommand = new Mock<_ICommand.ICommand>();
        ecommand.Setup(m => m.Execute()).Throws(new Exception());

        var mre = new ManualResetEvent(false);
        var ss = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 4, () => { mre.Set(); }, q);
        var ss2 = IoC.Resolve<_ICommand.ICommand>("Soft Stop The Thread", 4, () => { }, q);

        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, command.Object).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, ss).Execute();
        IoC.Resolve<_ICommand.ICommand>("Send Command", 4, ecommand.Object).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => ss2.Execute());
        Assert.Empty(q);
    }

    [Fact]
    public void HashCodeProblems()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q);
        var hashcode = st.GetHashCode();
        var hashcode2 = st.GetHashCode();

        Assert.Equal(hashcode, hashcode2);
    }
}
