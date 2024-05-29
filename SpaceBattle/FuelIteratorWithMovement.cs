﻿namespace SpaceBattle;

public class FuelIteratorWithMovement : IStrategy
{
    public readonly IEnumerator<object> poit;

    public FuelIteratorWithMovement(IEnumerator<object> poit)
    {
        this.poit = poit;
    }

    public object Strategy(params object[] args)
    {
        var c = (int)poit.Current;
        poit.MoveNext();
        return c;
    }
}
