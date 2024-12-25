using UnityEngine;

public interface ILVariable
{
    public char Key { get; }

    public ILVariable Clone(LContext context);

    public void Generated();

    public void Dispose();
}