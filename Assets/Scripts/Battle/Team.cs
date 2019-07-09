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

        this.SetUp(shooters, 0.05f, 1f);
    }

    // ==============================================
    public void AllocateShooters(Transform[] spawns, Transform anchor)
    {
        int i = 0;
        int j = 0;

        while(i < spawns.Length && j < this.entities.Length)
        {
            var entity = this.entities[j];
            if (!entity.active)
            {
                this.entities[j].Activate(spawns[i].position);
                this.entities[j].SetAnchor(anchor);

                i++;
            }

            j++;
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

    // ==============================================
    public override float GetFitness(Shooter entity, int index)
    {
        return 0.5f;
    }
}
