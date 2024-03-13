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

        IoC.Resolve<ICommand>("Scopes.Current.Set",
            IoC.Resolve<object>("Scopes.New",
                IoC.Resolve<object>("Scopes.Root")
            )
        ).Execute();

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
    }
    [Fact]
    public void HardStopShouldStopServerThread()
    {

        IoC.Resolve<ICommand>("IoC.Register", "ExceptionHandler", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<_ICommand.ICommand>(10);
        var st = new ServerThread(q);

        var command = new Mock<_ICommand.ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        q.Add(command.Object);

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<_ICommand.ICommand>("Server.Commands.HardStop", st, () => { mre.Set(); });
        //IoC.Resolve<ICommand>("IoC.Register", "HardStopCommand", (object[] args) => new ActionCommand(() => { mre.Set(); })).Execute();

        q.Add(hs);
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
    //     var mre = new ManualResetEvent(false);
        
    //     var q = new BlockingCollection<_ICommand.ICommand>(10);
        
    //     var st = new ServerThread(q);

    //     var cmd = new Mock<_ICommand.ICommand>();
    //     cmd.Setup(m => m.Execute()).Verifiable();

    //     var hs = IoC.Resolve<ICommand>("Server.Commands.HardStop", st, () => { mre.Set(); });

    //     var handleCommand = new Mock<_ICommand.ICommand>();
    //     handleCommand.Setup(m => m.Execute()).Verifiable();
    //     // IoC.Resolve<ICommand>("IoC.Register", "Exception.Handler", (object[] args)=> handleCommand.Object).Execute();

    //     var cmdE = new Mock<_ICommand.ICommand>();
    //     cmdE.Setup(m => m.Execute()).Throws<Exception>().Verifiable();

    //     q.Add(
    //         IoC.Resolve<_ICommand.ICommand>("Scopes.Current.Set",
    //             IoC.Resolve<object>("Scopes.New",
    //                 IoC.Resolve<object>("Scopes.Root")
    //             )
    //         )
    //     );
    //     q.Add(IoC.Resolve<_ICommand.ICommand>("IoC.Register", "ExceptionHandler", (object[] args) => handleCommand.Object));
    //     q.Add(cmd.Object);
    //     q.Add(cmdE.Object);
    //     q.Add(cmd.Object);
    //     //q.Add(hs);
    //     q.Add(cmd.Object);

    //     st.Execute();
        
    //     mre.WaitOne();

    //     Assert.Single(q);
        
    //     cmd.Verify(m => m.Execute(), Times.Exactly(2));
    //     handleCommand.Verify(m => m.Execute(), Times.Once());

    // }
}

