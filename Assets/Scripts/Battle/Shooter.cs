using CerebroML;
using CerebroML.Factory;
using CerebroML.Genetics;
using CerebroML.Util;
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

    [Header("Movement")]
    public float movementSpeed = 2f;
    public float turnSpeed = 1f;

    private Vector3[] eyes;

    public bool active
    {
        get; protected set;
    }

    public int hits
    {
        get; protected set;
    }

    public float aliveTime
    {
        get; protected set;
    }

    public float walkedDistance
    {
        get { return this.positionRecorder.GetRecordedDistance(); }
    }

    public float friendsLookedPercent
    {
        get { return this.lookToFriend.getAvg(); }
    }

    public float foesLookedPercent
    {
        get { return this.lookToFoe.getAvg(); }
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

    private PositionRecorder positionRecorder;
    private FloatRecorder lookToFriend;
    private FloatRecorder lookToFoe;

    private NameIndexedBuffer sensorBuffer;
    private float[] reaction;

    private float tickTimer = 0;
    private float tickTimeOut = 0.5f;

    private float shootCoolDownTimer = 0;
    private float shootCoolDownTimeOut = 1.5f;

    private float health = 1f;

    private bool damaged;
    private Vector3 damageDirection;

    // =======================================
    void Awake()
    {
        // Sensor data
        /**
         * x
         * y
         * health_percent
         * Can shoot
         * forward_x
         * forward_y
         *
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

        this.sensorBuffer = new NameIndexedBuffer();

        this.sensorBuffer.AddIndex("position", 2);
        this.sensorBuffer.AddIndex("health", 1);
        this.sensorBuffer.AddIndex("shoot_time", 1);
        this.sensorBuffer.AddIndex("forward", 2);

        for (int i = 0; i < this.eyeCount; i++)
        {
            this.sensorBuffer.AddIndex($"eye_{i}", 3);
        }

        this.sensorBuffer.AddIndex("hit_from", 3);

        // Reaction data
        /**
         * forward
         * backwards
         * turn left
         * turn right
         * shoot
         */
        this.reaction = new float[5];

        this.brain = BrainFactory.Create()
            .WithInput(this.sensorBuffer.size)
            .WithLayer(16, LayerType.Tanh)
            .WithLayer(8, LayerType.Tanh)
            .WithLayer(this.reaction.Length, LayerType.Sigmoid)
            .WithWeightBiasAmplitude(10f)
            .Build();

        this.physics = GetComponent<Rigidbody>();
        this.positionRecorder = GetComponent<PositionRecorder>();
        this.lookToFriend = new FloatRecorder();
        this.lookToFoe = new FloatRecorder();
    }

    // =======================================
    void Update()
    {
        this.aliveTime += Time.deltaTime;
        this.tickTimer += Time.deltaTime;

        this.shootCoolDownTimer += Time.deltaTime;

        if (this.tickTimer > this.tickTimeOut)
        {
            this.tickTimer -= this.tickTimeOut;

            // Stats
            this.positionRecorder.Add(this.transform.position);

            // "Think"
            this.CollectSensorData();

            this.reaction = this.brain.Run(this.sensorBuffer.data);

            float shoot = this.reaction[4];

            if (shoot >= 0.5f && this.shootCoolDownTimer >= this.shootCoolDownTimeOut)
            {
                this.shootCoolDownTimer = 0f;

                RaycastHit hit;

                Vector3 position = this.transform.position;
                Vector3 end = position + this.transform.forward * this.viewDistance;

                if (Physics.Raycast(position, this.transform.forward, out hit, this.viewDistance))
                {
                    // What object is
                    var shooter = hit.collider.gameObject.GetComponent<Shooter>();

                    if (this.gameObject == hit.collider.gameObject)
                    {
                        Debug.LogError("It's me, Mario!");
                    }

                    if (shooter != null)
                    {
                        bool killed = shooter.ReceiveDamage(0.3f, this.transform.forward);

                        if (shooter.faction == this.faction)
                        {
                            // Friendly fire
                            this.hits -= killed ? 3 : 2;
                        }
                        else
                        {
                            this.hits += killed ? 2 : 1;
                        }
                    }

                    end = hit.point;
                }
                var go = PrefabPooling.current.Instantiate("shoot");
                ShootGfx s = go.GetComponent<ShootGfx>();

                s.start = position;
                s.end = end;
            }
        }

        float forward = this.reaction[0] - this.reaction[1];
        this.physics.velocity = this.transform.forward * forward * this.movementSpeed;

        float turn = this.reaction[2] - this.reaction[3];
        this.transform.Rotate(0, turn * this.turnSpeed * Time.deltaTime, 0);
    }

    // =======================================
    void CollectSensorData()
    {
        // Collect position and health
        this.sensorBuffer.SetData(
            "position",
            this.transform.position.x,
            this.transform.position.z
        );

        this.sensorBuffer.SetData("health", this.health);

        // 'Can shoot' timer
        this.sensorBuffer.SetData("shoot_time", Mathf.Min(this.shootCoolDownTimer / this.shootCoolDownTimeOut, 1f));
        this.sensorBuffer.SetData(
            "forward",
            this.transform.forward.x,
            this.transform.forward.z
        );

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

        int sideEyeCount = Mathf.FloorToInt(this.eyeCount / 2f);

        float fovPercent = this.fov / sideEyeCount;

        for (int i = -sideEyeCount; i <= sideEyeCount; i++)
        {
            float angle = baseAngle + (fovPercent * i);
            Vector3 lookDir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            this.eyes[index] = lookDir;
            index++;
        }

        // Detect what each eye 'sees' and put the data in the sensor
        // If nothing is seen, just put 0

        Vector3 position = this.transform.position;
        int eyeIndex = 0;

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

            if (Physics.Raycast(position + (lookDir * 0.5f), lookDir, out hit, this.viewDistance))
            {
                GameObject go = hit.collider.gameObject;

                Shooter sh = go.GetComponent<Shooter>();

                if (sh != null)
                {
                    float angle = Vector3.Angle(this.transform.forward, lookDir) * Mathf.Deg2Rad;
                    float percent = angle / this.fov;

                    if (sh.faction == this.faction)
                    {
                        // Friend
                        see = 1f;

                        this.lookToFriend.Add(percent);
                    }
                    else
                    {
                        // Enemy
                        see = -1;

                        this.lookToFoe.Add(1f - percent);
                    }
                }
                else
                {
                    // It's a wall.
                    see = .5f;
                }

                seeX = hit.point.x;
                seeZ = hit.point.z;
            }

            this.sensorBuffer.SetData($"eye_{eyeIndex}", see, seeX, seeZ);
            eyeIndex++;
        }
    }

    // =======================================
    void CollectHitSensorData()
    {
        int sensorIndex = 6 + this.eyes.Length * 3;

        this.sensorBuffer.SetData(
            "hit_from",
            this.damaged ? 1f : 0f,
            this.damageDirection.x,
            this.damageDirection.z
        );

        // Clear hit data
        this.damaged = false;
        this.damageDirection.Set(0, 0, 0);
    }

    // =======================================
    public bool ReceiveDamage(float damage, Vector3 direction)
    {
        this.health -= damage;
        this.damaged = true;
        this.damageDirection = direction;

        if (this.health <= 0f)
        {
            this.health = 0;

            this.Deactivate();

            // I am dead :(
            return true;
        }

        return false;
    }

    // =======================================
    public void SetAnchor(Transform parent)
    {
        this.transform.parent = parent;
    }

    // =======================================
    public void Activate(Transform activationPoint)
    {
        // Stats
        this.hits = 0;
        this.aliveTime = 0f;
        if (this.positionRecorder != null)
        {
            this.positionRecorder.Reset();

            this.lookToFriend.Reset();
            this.lookToFoe.Reset();
        }

        this.health = 1f;
        this.damaged = false;

        this.active = true;
        this.gameObject.SetActive(true);

        this.transform.rotation = activationPoint.rotation;

        float bilinear = Random.Range(-1, 1);
        this.transform.position = activationPoint.position + Vector3.right * bilinear;
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
