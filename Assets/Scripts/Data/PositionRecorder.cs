using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRecorder : MonoBehaviour
{
    public float marginBetweenPoints = .5f;

    private List<Vector3> points = new List<Vector3>();

    private float distance = 0f;

    // ===========================================
    public void Add(Vector3 point)
    {
        StartCoroutine(this.AddRutine(point));
    }

    // ===========================================
    IEnumerator AddRutine(Vector3 point)
    {
        bool canAdd = true;

        int length = this.points.Count;
        int i = 0;

        while(i < length && canAdd)
        {
            Vector3 stored = this.points[i];
            float distance = Vector3.Distance(stored, point);

            if (distance < this.marginBetweenPoints)
                canAdd = false;

            i++;

            if (i % 10 == 0)
                yield return null;
        }

        if (canAdd)
            this.points.Add(point);

        yield return null;
    }

    // ===========================================
    public float GetRecordedDistance()
    {
        // Cache the distance for consecutive calls
        if (this.distance > 0f)
            return this.distance;

        int length = this.points.Count - 1;

        float dist = 0f;

        for (int i = 0; i < length; i++)
        {
            dist += Vector3.Distance(this.points[i], this.points[i +1 ]);
        }

        this.distance = dist;

        return dist;
    }

    // ===========================================
    public void Reset()
    {
        this.distance = 0f;
        this.points.Clear();
    }
}
