using UnityEngine;

public class ShootGfx : MonoBehaviour
{
    public Vector3 start;
    public Vector3 end;

    private LineRenderer line;

    void OnEnable()
    {
        Invoke("PseudoDestroy", .3f);

        if (this.line == null) { this.line = GetComponent<LineRenderer>(); }

        this.line.positionCount = 2;

        this.line.SetPosition(0, start);
        this.line.SetPosition(1, end);
    }

    void PseudoDestroy()
    {
        this.gameObject.SetActive(false);
    }
}
