namespace SpaceBattle;

interface IMovable 
{
    Vector Location{get; set;}
    Vector Velosity{get;}
}
public class Vector{

    private readonly int[] _values;
    public int this[int index]{
        get => _values[index];
        set => _values[index] = value;
    }

    public Vector(params int[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));
        _values = values;
    }

    public static Vector operator +(Vector v1,Vector v2){
        var _vector = new Vector(2);
        _vector[0] = v1[0] + v2[0];
        _vector[1] = v1[1] + v2[1];
        return _vector;
    }

    public override string ToString()
    {
        return $"<{string.Join(", ", _values)}>";
    }
}


public class Move
{
    IMovable movable;

    public void Execute(){
        movable.Location+=movable.Velosity;
    }
}
