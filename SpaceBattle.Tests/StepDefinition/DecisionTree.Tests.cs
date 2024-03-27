using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Test;

public class DecisionTreesTests
{

    public DecisionTreesTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();
    }

    [Fact]
    public void PositiveBuildingDecisionTreesTest()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var path = "../../../trees.txt";
        var getDecisionTreesStrategy = new Mock<IStrategy>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.GetDecisionTrees", (object[] args) => getDecisionTreesStrategy.Object.Strategy(args)).Execute();
        getDecisionTreesStrategy.Setup(t => t.Strategy(It.IsAny<object[]>())).Returns(new Dictionary<int, object>()).Verifiable();

        var bdts = new BuildingDecisionTrees(path);

        bdts.Execute();

        getDecisionTreesStrategy.Verify();
    }

    [Fact]
    public void NegativeBuildingDecisionTreesTestThrowsException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var path = "";
        var getDecisionTreesStrategy = new Mock<IStrategy>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.GetDecisionTrees", (object[] args) => getDecisionTreesStrategy.Object.Strategy(args)).Execute();
        getDecisionTreesStrategy.Setup(t => t.Strategy(It.IsAny<object[]>())).Returns(new Dictionary<int, object>()).Verifiable();

        var bdts = new BuildingDecisionTrees(path);

        Assert.Throws<Exception>(() => bdts.Execute());

        getDecisionTreesStrategy.Verify();
    }

    [Fact]
    public void NegativeBuildingDecisionTreesTestThrowsFileNotFoundException()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        
        var path = "./DT_File.txt";
        var getDecisionTreesStrategy = new Mock<IStrategy>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.GetDecisionTrees", (object[] args) => getDecisionTreesStrategy.Object.Strategy(args)).Execute();
        getDecisionTreesStrategy.Setup(t => t.Strategy(It.IsAny<object[]>())).Returns(new Dictionary<int, object>()).Verifiable();

        var bdts = new BuildingDecisionTrees(path);

        Assert.Throws<FileNotFoundException>(() => bdts.Execute());

        getDecisionTreesStrategy.Verify();
    }
}
