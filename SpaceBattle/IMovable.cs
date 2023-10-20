namespace _IMovable;
using Vector;

public interface IMovable
{
    Vector Location { get; set; }
    Vector Velosity { get; }
}
