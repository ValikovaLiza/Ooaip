﻿using _Vector;

namespace SpaceBattle;

public class PositionIterWithMovement : IStrategy
{
    public IEnumerator<object> poit;

    public PositionIterWithMovement(IEnumerator<object> poit)
    {
        this.poit = poit;
    }

    public object Strategy(params object[] args)
    {
        var c = (Vector)poit.Current;
        var m = poit.MoveNext();
        if (!m)
        {
            poit.Reset();
        }

        return c;
    }
}
