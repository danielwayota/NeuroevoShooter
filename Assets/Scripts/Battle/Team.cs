using CerebroML.Genetics;
using UnityEngine;

public class Team : Population<Shooter>
{
    private Faction faction;

    // ==============================================
    public Team(int size, Faction faction, GameObject shooterPrefab)
    {
        this.faction = faction;

        Shooter[] shooters = new Shooter[size];

        for (int i = 0; i < size; i++)
        {
            var obj = GameObject.Instantiate(shooterPrefab);

            var tmp = obj.GetComponent<Shooter>();

            if (tmp == null)
            {
                Debug.LogError("Found shooter with no Shooter component.");
                continue;
            }
            shooters[i] = tmp;
            tmp.faction = faction;
        }

        this.SetUp(shooters, 0.1f, 10f);
    }

    // ==============================================
    public void AllocateShooters(Transform[] spawns, Transform anchor)
    {
        int i = 0;
        int j = 0;

        this.Shuffle();

        while(i < spawns.Length && j < this.entities.Length)
        {
            var entity = this.entities[j];
            if (!entity.active)
            {
                this.entities[j].Activate(spawns[i]);
                this.entities[j].SetAnchor(anchor);

                i++;
            }

            j++;
        }
    }

    // ==============================================
    public void Shuffle()
    {
        for (int i = 0; i < this.entities.Length - 1; i++)
        {
            int index = Random.Range(i + 1, this.entities.Length);

            var aux = this.entities[index];
            this.entities[index] = this.entities[i];
            this.entities[i] = aux;
        }
    }

    // ==============================================
    public void KillAll()
    {
        foreach (var shooter in this.entities)
        {
            shooter.Deactivate();
        }
    }

    // ==============================================
    public bool AreAllDead()
    {
        int i = 0;
        bool allDead = true;

        while(i < this.entities.Length && allDead)
        {
            if (entities[i].active)
            {
                allDead = false;
            }

            i++;
        }

        return allDead;
    }

    // ONLY TIME AND HITS

    // float maxHits = 0;
    // float maxAliveTime = 0;

    // // ==============================================
    // public override void OnBeforeNextGeneration()
    // {
    //     this.maxHits = 0;
    //     this.maxAliveTime = 0;

    //     foreach (var shooter in this.entities)
    //     {
    //         this.maxHits = Mathf.Max(shooter.hits, this.maxHits);
    //         this.maxAliveTime = Mathf.Max(shooter.aliveTime, this.maxAliveTime);
    //     }

    //     // Avoid zero division
    //     this.maxHits = Mathf.Max(1, this.maxHits);
    //     this.maxAliveTime = Mathf.Max(1, this.maxAliveTime);
    // }

    // // ==============================================
    // public override float GetFitness(Shooter entity, int index)
    // {
    //     float alive = entity.aliveTime / this.maxAliveTime;

    //     float kills = entity.hits / this.maxHits;

    //     float fitness = (alive * 0.5f) + (kills * 0.5f);

    //     return Mathf.Pow(fitness, 2);
    // }

    // // ONLY TIME, HITS AND DISTANCE

    // float maxHits = 0;
    // float maxAliveTime = 0;
    // float maxDistance = 0;

    // // ==============================================
    // public override void OnBeforeNextGeneration()
    // {
    //     this.maxHits = 0;
    //     this.maxAliveTime = 0;
    //     this.maxDistance = 0;

    //     foreach (var shooter in this.entities)
    //     {
    //         this.maxHits = Mathf.Max(shooter.hits, this.maxHits);
    //         this.maxAliveTime = Mathf.Max(shooter.aliveTime, this.maxAliveTime);
    //         this.maxDistance = Mathf.Max(shooter.walkedDistance, this.maxDistance);
    //     }

    //     // Avoid zero division
    //     this.maxHits = Mathf.Max(1, this.maxHits);
    //     this.maxAliveTime = Mathf.Max(1, this.maxAliveTime);
    //     this.maxDistance = Mathf.Max(1, this.maxDistance);
    // }

    // // ==============================================
    // public override float GetFitness(Shooter entity, int index)
    // {
    //     float alive = entity.aliveTime / this.maxAliveTime;
    //     float kills = entity.hits / this.maxHits;
    //     float walked = entity.walkedDistance / this.maxDistance;

    //     float fitness = (alive * .2f) + (walked * .3f) + (kills * .5f);

    //     return Mathf.Pow(fitness, 2);
    // }

    // TIME, HITS, DISTANCE AND 'LOOK TIMES'

    float maxHits = 0;
    float maxAliveTime = 0;
    float maxDistance = 0;

    // ==============================================
    public override void OnBeforeNextGeneration()
    {
        this.maxHits = 0;
        this.maxAliveTime = 0;
        this.maxDistance = 0;

        foreach (var shooter in this.entities)
        {
            this.maxHits = Mathf.Max(shooter.hits, this.maxHits);
            this.maxAliveTime = Mathf.Max(shooter.aliveTime, this.maxAliveTime);
            this.maxDistance = Mathf.Max(shooter.walkedDistance, this.maxDistance);
        }

        // Avoid zero division
        this.maxHits = Mathf.Max(1, this.maxHits);
        this.maxAliveTime = Mathf.Max(1, this.maxAliveTime);
        this.maxDistance = Mathf.Max(1, this.maxDistance);
    }

    // ==============================================
    public override float GetFitness(Shooter entity, int index)
    {
        float alive = entity.aliveTime / this.maxAliveTime;
        float hits = entity.hits / this.maxHits;
        float walked = entity.walkedDistance / this.maxDistance;

        float lookToFriend = entity.friendsLookedPercent;
        float lookToFoe = entity.foesLookedPercent;

        float fitness = (alive * .05f) + (walked * .15f) + (lookToFriend * .2f) + (lookToFoe * .2f) + (hits * .4f);

        return Mathf.Pow(fitness, 2);
    }
}
