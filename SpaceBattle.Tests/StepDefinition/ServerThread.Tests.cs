using Hwdtech;
using Hwdtech.Ioc;

namespace SpaceBattle.Test;
public class ServerTheardTests
{
    public ServerTheardTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();
    }

    [Fact]
    public void SoftStop()
    {

    }

    [Fact]
    public void HardStop()
    {

    }
}
