using UnityEngine;
using UnityEngine.UI;

public class VibrationVisualizer : MonoBehaviour
{
    public Image[] bars;
    [SerializeField] float maxVibrationValue = 150;

    public void SetVibrationValue(float value)
    {
        float fillRatio = value / maxVibrationValue;

        foreach(Image bar in bars)
        {
            bar.fillAmount = fillRatio;
        }
    }
}
