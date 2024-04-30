namespace SpaceBattle.Lib.Test;
using System.Collections.Generic;
using Hwdtech;
using Xunit;

public class CreateDeleteGameTest
{
    Dictionary<string, object> scopeMap = new Dictionary<string, object>() {
        {"scope1", 123},
        {"scope2", 321},
        {"scope3", 132}
    };
    public CreateDeleteGameTest()
    {
        new Hwdtech.Ioc.InitScopeBasedIoCImplementationCommand();
        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));
        IoC.Resolve<ICommand>("Scopes.Current.Set", scope).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Game.Get.GamesDictioanary", (object[] args) =>
        {
            return new Dictionary<string, Queue<Lib.ICommand>>();
        }).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Game.ScopeMap", (object[] args) =>
        {
            return scopeMap;
        }).Execute();
    }
    [Fact]
    public void Test_DeleteGame()
    {
        string gameId = "scope2";
        var deleteCommand = new DeleteGameCommand(gameId);
        deleteCommand.Execute();

        var scopeMap = IoC.Resolve<Dictionary<string, object>>("Game.ScopeMap");
        Assert.False(scopeMap.ContainsKey(gameId));

    }
    [Fact]
    public void Test_CreateGame()
    {
        string gameId = "123";

        var createGameStrategy = new CreateNewStrategy();
        var result = createGameStrategy.Execute(gameId);

        Assert.NotNull(result);
        Assert.IsType<GameCommand>(result);
    }
}