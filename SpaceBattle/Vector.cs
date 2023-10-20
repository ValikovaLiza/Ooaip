namespace Vector;
public class Vector
{
    private readonly int[] _values;
    public int this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }

    public Vector(params int[] values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
    }

    public static Vector operator +(Vector v1, Vector v2)
    {
        var _vector = new Vector(2);
        _vector[0] = v1[0] + v2[0];
        _vector[1] = v1[1] + v2[1];
        return _vector;
    }

    public static Vector operator ==(Vector v1, Vector v2)
    {
        var _vectorEq = new Vector(2);
        if ((v1[0] == v2[0]) && (v1[1] == v2[1]))
        {
            _vectorEq[0] = 1;
            _vectorEq[1] = 1;
        }
        else
        {
            _vectorEq[0] = 0;
            _vectorEq[1] = 0;
        }

        return _vectorEq;
    }

    public static Vector operator !=(Vector v1, Vector v2)
    {
        var _vectorNEq = new Vector(2);
        if ((v1[0] != v2[0]) || (v1[1] != v2[1]))
        {
            _vectorNEq[0] = 0;
            _vectorNEq[1] = 0;
        }

        return _vectorNEq;
    }

    public override string ToString()
    {
        return $"<{string.Join(", ", _values)}>";
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }
}
