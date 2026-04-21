using UnityEngine;
using UnityEngine.UI;
using System;

public class VibrationVisualizer : MonoBehaviour
{
    public Image[] bars;
    [SerializeField] float maxVibrationValue = 150;
    [SerializeField] MaterialVisualizer[] materials;

    public void SetVibrationValue(float value)
    {
        float fillRatio = value / maxVibrationValue;

        foreach(Image bar in bars)
        {
            bar.fillAmount = fillRatio;
        }

        foreach(MaterialVisualizer material in materials)
        {
            material.SetIntensityValue(fillRatio);
        }
    }
}

[Serializable]
public class MaterialVisualizer
{
    public MeshRenderer meshRenderer;
    public int materialIndex;
    public Vector2 intensityRange;
    public string propertyName = "_Glow_intensity";

    public void SetIntensityValue(float value)
    {
        float intensity = Mathf.Lerp(intensityRange.x, intensityRange.y, value);
        Material mat = meshRenderer.materials[materialIndex];
        if (mat == null)
        { 
            Debug.LogWarning("Material Not found"); 
            return; 
        }
        if (mat.HasProperty(propertyName))
        {
            mat.SetFloat(propertyName, intensity);
        }
        else
        {
            Debug.Log($"Property {propertyName} not found on material {mat.name}");
        }
    }
}
