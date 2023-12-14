namespace SpaceBattle;
using _Vector;

public interface IMovable
{
    Vector Location { get; set; }
    Vector Velosity { get; }
}
