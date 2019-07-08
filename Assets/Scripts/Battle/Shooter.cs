using CerebroML;
using CerebroML.Genetics;
using UnityEngine;

public class Shooter : MonoBehaviour, IEntity
{
    public Renderer body;

    public bool active
    {
        get; protected set;
    }

    private Faction _faction;
    public Faction faction
    {
        get { return this._faction; }
        set {
            this._faction = value;

            this.body.material = this._faction.material;
        }
    }
    private Cerebro brain;

    // =======================================
    void Start()
    {

    }

    // =======================================
    public void SetAnchor(Transform parent)
    {
        this.transform.parent = parent;
    }

    // =======================================
    public void Activate(Vector3 position)
    {
        this.active = true;
        this.gameObject.SetActive(true);

        this.transform.position = position;
    }

    // =======================================
    public void Deactivate()
    {
        this.active = false;
        this.gameObject.SetActive(false);
    }

    // =======================================
    public Genome GetGenome()
    {
        throw new System.NotImplementedException();
    }

    // =======================================
    public void SetGenome(Genome g)
    {
        throw new System.NotImplementedException();
    }
}
