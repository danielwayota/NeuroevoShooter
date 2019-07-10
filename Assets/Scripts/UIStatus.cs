using UnityEngine;
using UnityEngine.UI;

public class UIStatus : MonoBehaviour
{
    public Text generationText;
    public Text generationTimeText;

    public void SetGeneration(int gen)
    {
        this.generationText.text = gen.ToString();
    }

    public void SetGenerationTime(float time)
    {
        this.generationTimeText.text = $"{time:0.0}s";
    }
}
