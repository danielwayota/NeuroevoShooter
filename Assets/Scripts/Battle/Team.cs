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

        this.SetUp(shooters, 0.05f, 10f);
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

    float maxHits = 0;
    float maxAliveTime = 0;

    // ==============================================
    public override void OnBeforeNextGeneration()
    {
        this.maxHits = 0;
        this.maxAliveTime = 0;

        foreach (var shooter in this.entities)
        {
            this.maxHits = Mathf.Max(shooter.hits, this.maxHits);
            this.maxAliveTime = Mathf.Max(shooter.aliveTime, this.maxAliveTime);
        }

        // Avoid zero division
        this.maxHits = Mathf.Max(1, this.maxHits);
        this.maxAliveTime = Mathf.Max(1, this.maxAliveTime);
    }

    // ==============================================
    public override float GetFitness(Shooter entity, int index)
    {
        float alive = entity.aliveTime / this.maxAliveTime;

        float kills = entity.hits / this.maxHits;

        float fitness = (alive * 0.5f) + (kills * 0.5f);

        return Mathf.Pow(fitness, 2);
    }
}
