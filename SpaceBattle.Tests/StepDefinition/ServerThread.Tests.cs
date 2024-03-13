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

        IoC.Resolve<ICommand>("IoC.Register",
            "Server.Commands.HardStop",
            (object[] args) =>
            {
                var thread = (ServerThread)args[0];
                var action = (Action)args[1];
                return new ActionCommand(
                    () =>
                    {
                        new HardStop(thread).Execute();
                        new ActionCommand(action).Execute();
                    }
                );
            }
        ).Execute();

        IoC.Resolve<ICommand>("IoC.Register",
            "Server.Commands.SoftStop",
            (object[] args) =>
            {
                var thread = (ServerThread)args[0];
                var action = (Action)args[1];
                var queue = (BlockingCollection<_ICommand.ICommand>)args[2];
                return new ActionCommand(
                    () =>
                    {
                        new SoftStop(thread, action, queue).Execute();
                        new ActionCommand(action).Execute();
                    }
                );
            }
        ).Execute();
    }
    [Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.Root"));

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        q.Add(command.Object);

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<_ICommand.ICommand>("Server.Commands.HardStop", st, () => { mre.Set(); });
        
        q.Add(hs);
        q.Add(command.Object);

        st.Execute();
        Assert.True(mre.WaitOne(1000));

        Assert.Single(q);
        command.Verify(m => m.Execute(), Times.Once);
        //Assert.True(q.TryAdd(command.Object));
        //Assert.True(st.Stop());
    }

    [Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.Root"));

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        q.Add(command.Object);

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<_ICommand.ICommand>("Server.Commands.SoftStop", st, () => { mre.Set(); }, q);

        q.Add(ss);
        q.Add(command.Object);

        st.Execute();
        Assert.True(mre.WaitOne(1000));

        Assert.Single(q);
        command.Verify(m => m.Execute(), Times.Once);
        //Assert.True(q.TryAdd(command.Object));
        //Assert.True(st.Stop());
    }

    // [Fact]
    // public void ExceptionCommandShouldNotStopServerThread()
    // {

    //     IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.Root"));

    //     var mre = new ManualResetEvent(false);
    //     var q = new BlockingCollection<_ICommand.ICommand>(10);
    //     var st = new ServerThread(q);

    //     var command = new Mock<_ICommand.ICommand>();
    //     command.Setup(m => m.Execute()).Verifiable();

    //     var hs = IoC.Resolve<_ICommand.ICommand>("Server.Commands.HardStop", st, () => { mre.Set(); });

    //     var handleCommand = new Mock<_ICommand.ICommand>();
    //     handleCommand.Setup(m => m.Execute()).Verifiable();

    //     var cmdE = new Mock<_ICommand.ICommand>();
    //     cmdE.Setup(m => m.Execute()).Throws<Exception>().Verifiable();

    //     q.Add(IoC.Resolve<_ICommand.ICommand>("Scopes.Current.Set",
    //             IoC.Resolve<object>("Scopes.New",
    //                 IoC.Resolve<object>("Scopes.Root")
    //             )
    //         )
    //     );
    //     q.Add(IoC.Resolve<_ICommand.ICommand>("IoC.Register", "ExceptionHandler", (object[] args) => handleCommand.Object));
    //     q.Add(command.Object);
    //     q.Add(cmdE.Object);
    //     q.Add(command.Object);
    //     q.Add(hs);
    //     q.Add(command.Object);

    //     st.Execute();

    //     mre.WaitOne(1000);

    //     Assert.Single(q);

    //     command.Verify(m => m.Execute(), Times.Exactly(2));
    //     handleCommand.Verify(m => m.Execute(), Times.Once());

    // }
}

