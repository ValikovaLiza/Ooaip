namespace _Vector;
public class Vector
{
    private readonly int[] _values;
    public int this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }
    public int Size => _values.Length;

    public Vector(params int[] values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException();
        }

        _values = values;
    }

    public static Vector operator +(Vector v1, Vector v2)
    {
        if (v1.Size != v2.Size)
        {
            throw new System.ArgumentException();
        }

        var i = 0;
        var size = v1.Size;
        while (i < size)
        {
            v1._values[i] += v2._values[i];
            i++;
        }

        return v1;
    }

    public static Vector operator -(Vector v1, Vector v2)
    {
        if (v1.Size != v2.Size)
        {
            throw new System.ArgumentException();
        }

        var i = 0;
        var size = v1.Size;
        while (i < size)
        {
            v1._values[i] -= v2._values[i];
            i++;
        }

        return v1;
    }

    public static bool operator ==(Vector v1, Vector v2)
    {
        if (v1.Size != v2.Size)
        {
            throw new System.ArgumentException();
        }

        return v1.Equals(v2);
    }

    public static bool operator !=(Vector v1, Vector v2)
    {
        if (v1.Size != v2.Size)
        {
            throw new System.ArgumentException();
        }

        return !(v1 == v2);
    }
    public override bool Equals(object? obj)
    {
        return obj is Vector vector && _values.SequenceEqual(vector._values);
    }
    public override int GetHashCode()
    {
        return 0;
    }
}
