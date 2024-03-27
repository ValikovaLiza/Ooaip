using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Test;

public class MacroCommandsTest
{
    public MacroCommandsTest()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var commandMock = new Mock<_ICommand.ICommand>();
        commandMock.Setup(c => c.Execute());

        var propertyMock = new Mock<IStrategy>();
        propertyMock.Setup(s => s.Strategy(It.IsAny<object[]>())).Returns(commandMock.Object);

        var listMock = new Mock<IStrategy>();
        listMock.Setup(l => l.Strategy()).Returns(new string[] { "Second" });

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.Operation.First", (object[] p) => listMock.Object.Strategy(p)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Second", (object[] p) => propertyMock.Object.Strategy(p)).Execute();
    }

    [Fact]
    public void TestPositiveMacroCommand()
    {
        var obj = new Mock<IUObject>();
        var newmc = new MacroCommandStrategy();

        var macrocommand = (_ICommand.ICommand)newmc.Strategy("First", obj.Object);

        macrocommand.Execute();
    }
}
