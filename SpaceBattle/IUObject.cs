﻿namespace SpaceBattle;

public interface IUObject
{
    void SetProperty(string key, object value);
    object GetProperty(string key);
}
