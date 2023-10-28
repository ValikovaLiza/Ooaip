namespace Vector;
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
        _values = values ?? throw new ArgumentNullException(nameof(values));
    }

    public static Vector operator +(Vector v1, Vector v2)
    {
        v1._values[0] += v2._values[0];
        v1._values[1] += v2._values[1];
        return v1;
    }

    public static Vector operator -(Vector v1, Vector v2)
    {
        v1._values[0] -= v2._values[0];
        v1._values[1] -= v2._values[1];
        return v1;
    }

    public override string ToString()
    {
        return $"<{string.Join(", ", _values)}>";
    }

    public static bool operator ==(Vector v1, Vector v2)
    {
        bool v1Null = ReferenceEquals(v1, null), v2Null = ReferenceEquals(v1, null);
        if (v1Null || v2Null)
        {
            return v1Null && v2Null;
        }

        return ReferenceEquals(v1, v2) || v1.Size == v2.Size && v1._values.SequenceEqual(v2._values);
    }

    public static bool operator !=(Vector v1, Vector v2)
    {
        return !(v1 == v2);
    }
    public override bool Equals(object? obj)
    {
        return obj is Vector vector && _values.SequenceEqual(vector._values);
    }
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
