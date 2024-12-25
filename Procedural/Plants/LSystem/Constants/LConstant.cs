using System;

[Serializable]
public abstract class LConstant<T>
{
    public abstract char Key { get; }

    public abstract LContext Apply(LContext context);
}