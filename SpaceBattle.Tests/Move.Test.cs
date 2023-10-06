namespace SpaceBattle.Tests;

using System.Diagnostics.CodeAnalysis;
using SpaceBattle;
using TechTalk.SpecFlow;

[Binding]
public class MoveTest
{
    private Vector location = new Vector();
    private Vector velosity = new Vector();
    private SpaceBattle spaceship = new SpaceBattle();
    [When("происходит прямолинейное равномерное движение без деформации")]
    public void CalculatedTheMovement()
    {
        try 
        {
            location = spaceship.Execute();
        }
        catch (Exception e)
        {
            
        }
    }

    [Given(@"космический корабль находится в точке пространства с координатами \((.*), (.*)\)")]
    public void GivenThePosition(Vector loc) 
    {
        Vector v1= loc;
    }

    [Given(@"имеет мгновенную скорость \((.*), (.*)\)")]
    public void GivenSpeed(Vector vel) 
    {
        Vector v2= vel;
    }

    [Given("изменить положение в пространстве космического корабля невозможно")]
    public void NotSetPosition() 
    {
        
    }

    [Given("скорость корабля определить невозможно")]
    public void NotSetSpeed() 
    {
        
    }


    [Then(@"космический корабль перемещается в точку пространства с координатами \((.*), (.*)\)")]
    public void MovingToAPoint(Vector v1, Vector v2)
    {
        location = spaceship.Execute(v1, v2);
    }
    
}