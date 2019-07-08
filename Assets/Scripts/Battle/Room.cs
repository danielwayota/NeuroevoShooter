using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public FactionSpawns[] factionSpawns;

    public Transform shootersAnchor;

    private Dictionary<Faction, Transform[]> spawns;

    // ===================================
    public void Initialize()
    {
        this.spawns = new Dictionary<Faction, Transform[]>();

        foreach (var facSpawn in this.factionSpawns)
        {
            this.spawns.Add(facSpawn.faction, facSpawn.spawns);
        }
    }

    // ===================================
    public Transform[] GetFactionSpawns(Faction faction)
    {
        return this.spawns[faction];
    }

    // ===================================
    public int GetFactionSize(Faction faction)
    {
        return this.spawns[faction].Length;
    }
}

[System.Serializable]
public struct FactionSpawns
{
    public Faction faction;
    public Transform[] spawns;
}
