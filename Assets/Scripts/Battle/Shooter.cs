using CerebroML;
using CerebroML.Factory;
using CerebroML.Genetics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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

    private Rigidbody physics;

    private float[] sensorData;

    // =======================================
    void Start()
    {
        this.sensorData = new float[3];

        this.brain = BrainFactory.Create()
            .WithInput(this.sensorData.Length)
            .WithLayer(3, LayerType.Tanh)
            .WithLayer(3, LayerType.Sigmoid)
            .Build();

        this.physics = GetComponent<Rigidbody>();
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
        return this.brain.GetGenome();
    }

    // =======================================
    public void SetGenome(Genome g)
    {
        this.brain.SetGenome(g);
    }
}
