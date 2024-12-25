using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private BodyVariable trunk;
    [SerializeField] private BodyVariable leaf;
    [SerializeField] private BodyVariable petal;

    [SerializeField] private LSystem<GameObject> lsystem;

    [SerializeField] private OffsetConstant trunkOffsetConstant;
    [SerializeField] private OffsetConstant rotationOffsetConstant;
    [SerializeField] private OffsetConstant branchOffsetConstant;

    [SerializeField] private LeafVariable leafVariable;
    [SerializeField] private TrunkVariable trunkVariable;

    public int iterations;

    private void Awake()
    {
        lsystem.Initialize(new ILVariable[] { trunkVariable, leafVariable }, new LConstant<GameObject>[] { trunkOffsetConstant, rotationOffsetConstant, branchOffsetConstant },
          () => new LContext()
          {
              position = transform.position,
              rotation = transform.rotation
          });

        lsystem.Iterate(iterations);
    }

    private float elapsedTime = 0;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 0.1f)
        {
            elapsedTime = 0;
            lsystem.Iterate(0);
        }
    }
}