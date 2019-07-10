using UnityEngine;

public class ShootGfx : MonoBehaviour
{
    public Vector3 start;
    public Vector3 end;

    void Start()
    {
        Destroy(this.gameObject, .3f);

        LineRenderer line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}
