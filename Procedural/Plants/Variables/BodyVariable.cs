using UnityEngine;

public class BodyVariable : MonoBehaviour, ILVariable
{
    public char Key => key;

    public string Rule => rule;

    [SerializeField] private string rule = "";

    public GameObject Value => gameObject;

    [SerializeField] private char key = 'C';

    public ILVariable Clone(LContext context)
    {
        var clone = Instantiate(this);

        //if (context.parent != null)
        //    clone.transform.SetParent(context.parent.Value.transform);

        clone.transform.position = context.position;
        clone.transform.rotation = context.rotation;
        return clone;
    }

    public void Dispose()
    {
        Destroy(gameObject);
    }

    public void Generated()
    {
    }
}