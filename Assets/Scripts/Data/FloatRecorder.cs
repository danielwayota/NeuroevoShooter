using System.Collections.Generic;

public class FloatRecorder
{
    private List<float> stats;

    // ======================================
    public FloatRecorder()
    {
        this.stats = new List<float>();
    }

    // ======================================
    public void Add(float num)
    {
        this.stats.Add(num);
    }

    // ======================================
    public void Reset()
    {
        this.stats.Clear();
    }

    // ======================================
    public float getAvg()
    {
        float sum = 0f;

        foreach (var n in this.stats)
            sum += n;

        float avg = sum / this.stats.Count;

        return avg;
    }
}