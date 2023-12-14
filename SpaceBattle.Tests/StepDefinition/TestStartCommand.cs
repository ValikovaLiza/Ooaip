namespace SpaceBattle.Tests;

using Hwdtech;
using Hwdtech.Ioc;
using Moq;

public class ActionCommand : ICommand
{
    private readonly Action _action;
    public ActionCommand(Action action) => _action = action;
    public void Execute()
    {
        _action();
    }
}

public class StartMoveCommandTests
{
    private readonly Mock<IMoveCommandStartable> _moveCommandStartableMock;
    private readonly Mock<IUObject> _uObjectMock;
    private readonly StartMoveCommand _startMoveCommand;

    public StartMoveCommandTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        _moveCommandStartableMock = new Mock<IMoveCommandStartable>();
        _uObjectMock = new Mock<IUObject>();

        _moveCommandStartableMock.Setup(m => m.UObject).Returns(_uObjectMock.Object);
        _moveCommandStartableMock.Setup(m => m.Dict).Returns(new Dictionary<string, object>());

        _startMoveCommand = new StartMoveCommand(_moveCommandStartableMock.Object);
    }

    [Fact]
    public void Execute_RegistersTargetsAndPushesMovingCommand_WhenCalled()
    {
        var movingCommandMock = new Mock<SpaceBattle.ICommand>();
        var commandMock = new Mock<SpaceBattle.ICommand>();
        var queueMock = new Mock<IQueue>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Moving.Commands", (object[] args) => movingCommandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Target.Register", (object[] args) => commandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Queue.Push", (object[] args) => queueMock.Object).Execute();

        _startMoveCommand.Execute();

        _moveCommandStartableMock.Verify(m => m.Dict, Times.Once());
        queueMock.Verify(q => q.Add(It.IsAny<SpaceBattle.ICommand>()), Times.Once());
    }
}
