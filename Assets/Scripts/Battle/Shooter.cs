using CerebroML;
using CerebroML.Factory;
using CerebroML.Genetics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Shooter : MonoBehaviour, IEntity
{
    [Header("Rendering")]
    public Renderer body;

    [Header("Perception")]
    public float fov = 1f;
    public int eyeCount = 3;
    public float viewDistance = 8f;

    private Vector3[] eyes;

    public bool active
    {
        get; protected set;
    }

    public int kills
    {
        get; protected set;
    }

    public float aliveTime
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

    private float tickTimer = 0;
    private float tickTimeOut = 0.5f;

    // =======================================
    void Start()
    {
        /**
         * x
         * y
         * health_percent
         * eye_active
         * eye_x
         * eye_y
            .
            . drive by eyeCount
            .
         * hit_from_active
         * hit_from_x
         * hit_from_y
         */

        this.eyes = new Vector3[this.eyeCount];

        this.sensorData = new float[
            3 + this.eyes.Length * 3 + 3
        ];

        this.brain = BrainFactory.Create()
            .WithInput(this.sensorData.Length)
            .WithLayer(3, LayerType.Tanh)
            .WithLayer(3, LayerType.Sigmoid)
            .Build();

        this.physics = GetComponent<Rigidbody>();
    }

    // =======================================
    void Update()
    {
        this.aliveTime += Time.deltaTime;
        this.tickTimer += Time.deltaTime;

        if (this.tickTimer > this.tickTimeOut)
        {
            this.tickTimer -= this.tickTimeOut;

            // "Think"
            this.CollectSensorData();
        }
    }

    // =======================================
    void CollectSensorData()
    {
        // Collect position and health
        this.sensorData[0] = this.transform.position.x;
        this.sensorData[1] = this.transform.position.z;
        // TODO: Collect health

        this.CollectEyesSensorData();

        this.CollectHitSensorData();
    }

    // =======================================
    void CollectEyesSensorData()
    {
        // Calculate the eyes ray direction
        Vector3 front = this.transform.forward;
        float baseAngle = Mathf.Atan2(front.z, front.x);

        int index = 0;
        // Calculate the base angle and the offset for the three eyes

        int eyeOffset = Mathf.FloorToInt(this.eyeCount / 2f);

        float fovPercent = this.fov / eyeOffset;

        for (int i = -eyeOffset; i <= eyeOffset; i++)
        {
            float angle = baseAngle + (fovPercent * i);
            Vector3 lookDir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            this.eyes[index] = lookDir;
            index++;
        }

        // Detect what each eye 'sees' and put the data in the sensor
        // If nothing is seen, just put 0

        int sensorIndex = 3;
        Vector3 position = this.transform.position;

        foreach (var lookDir in this.eyes)
        {
            RaycastHit hit;
            // 'see' is the way to indicate the type of object the eye is seeing
            //  0: nothing
            // TODO:
            float see = 0;

            // The seen object position
            float seeX = 0;
            float seeZ = 0;

            // Three front eyes
            if (Physics.Raycast(position, lookDir, out hit, this.viewDistance))
            {
                GameObject go = hit.collider.gameObject;

                // TODO: set some types
                see = 1;
                seeX = hit.point.x;
                seeZ = hit.point.z;
            }

            Debug.DrawLine(position, position + lookDir * this.viewDistance, Color.red, 1f );

            sensorData[sensorIndex + 0] = see;
            sensorData[sensorIndex + 1] = seeX;
            sensorData[sensorIndex + 2] = seeZ;
            sensorIndex += 3;
        }
    }

    // =======================================
    void CollectHitSensorData()
    {
        int sensorIndex = 3 + this.eyes.Length * 3;

        sensorData[sensorIndex + 0] = 0;
        sensorData[sensorIndex + 1] = 0;
        sensorData[sensorIndex + 2] = 0;
    }

    // =======================================
    public void SetAnchor(Transform parent)
    {
        this.transform.parent = parent;
    }

    // =======================================
    public void Activate(Vector3 position)
    {
        this.kills = 0;
        this.aliveTime = 0f;

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
