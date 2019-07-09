using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject shooterPrefab;
    public GameObject[] roomPrefabs;

    public float roomSize = 10f;

    public Faction red;
    public Faction green;

    [Range(1, 32)]
    public int roomCount = 1;

    private Room[] rooms;

    private Team redTeam;
    private Team greenTeam;

    // Timers
    float checkTimer = 0;
    float checkTimeOut = 0.5f;

    float generationTime = 0f;
    float maxGenerationTime = 1f;


    // ===================================
    void Start()
    {
        Vector3 position = this.transform.position;

        this.rooms = new Room[this.roomCount];

        int redFactionCount = 0;
        int greenFactionCount = 0;

        // Create rooms
        for (int i = 0; i < this.roomCount; i++)
        {
            // TODO: Pick a random room with some logic
            var obj = Instantiate(
                this.roomPrefabs[0],
                position + Vector3.right * this.roomSize *  i,
                Quaternion.identity
            );

            var tmp = obj.GetComponent<Room>();
            if (tmp == null)
            {
                Debug.LogError("Found room without Room component.");
                continue;
            }
            tmp.Initialize();
            this.rooms[i] = tmp;

            redFactionCount += tmp.GetFactionSize(this.red);
            greenFactionCount += tmp.GetFactionSize(this.green);
        }

        // Create each team
        this.redTeam = new Team(redFactionCount, this.red, this.shooterPrefab);
        this.greenTeam = new Team(greenFactionCount, this.green, this.shooterPrefab);

        // Allocate team members
        this.AllocateTeams();
    }

    // ===================================
    void Update()
    {
        this.checkTimer += Time.deltaTime;
        this.generationTime += Time.deltaTime;

        if (this.checkTimer > this.checkTimeOut)
        {
            this.checkTimer -= this.checkTimeOut;

            if (this.redTeam.AreAllDead() || this.greenTeam.AreAllDead())
            {
                this.generationTime = 0f;

                this.NextGeneration();
                this.AllocateTeams();
            }
        }

        if (this.generationTime > this.maxGenerationTime)
        {
            this.generationTime -= this.maxGenerationTime;

            this.NextGeneration();
            this.AllocateTeams();
        }
    }

    // ===================================
    void NextGeneration()
    {
        this.redTeam.KillAll();
        this.greenTeam.KillAll();

        this.redTeam.Next();
        this.greenTeam.Next();
    }

    // ===================================
    void AllocateTeams()
    {
        foreach (var room in this.rooms)
        {
            this.redTeam.AllocateShooters(
                room.GetFactionSpawns(this.red),
                room.shootersAnchor
            );
            this.greenTeam.AllocateShooters(
                room.GetFactionSpawns(this.green),
                room.shootersAnchor
            );
        }
    }
}