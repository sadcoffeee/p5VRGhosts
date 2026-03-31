using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

public class HauntableToy : MonoBehaviour
{
    //Settings
    [SerializeField] bool isHaunted = false;
    [SerializeField] GameObject spawnEffect;
    [SerializeField] GameObject freedEffect;
    [SerializeField] Material hauntedMaterial;
    [SerializeField] GameObject ExpelledGhost;

    //References
    [SerializeField] MeshRenderer meshRenderer;

    //Variables
    Material defaultMaterial;

    //Logic
    private void Start()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (!isHaunted)
        {
            defaultMaterial = meshRenderer.material;
        }
    }



    //Methods
    void UpdateMaterial()
    {
        if (isHaunted)
        {
            meshRenderer.material = hauntedMaterial;
        }
        else
        {
            meshRenderer.material = defaultMaterial;
        }
    }

    //Public methods
    public void StealToy()
    {
        HauntedToyManager.instance.AddToy(gameObject);
    }

    public void SetHaunted(bool state)
    {
        isHaunted = state;

        UpdateMaterial();

        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, transform.position, Quaternion.identity);
        }
    }

    public void ReleaseGhost()
    {
        SetHaunted(false);
        //TODO: Spawn ghost
        if (freedEffect != null)
        {
            Instantiate(freedEffect, transform.position, Quaternion.identity);
        }
    }
}


//Custom Editor
[CustomEditor(typeof(HauntableToy))]
[CanEditMultipleObjects]
public class HauntableToyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Debug tools");

        HauntableToy hauntableToy = (HauntableToy)target;
        if (GUILayout.Button("Steal Toy"))
        {
            hauntableToy.StealToy();
        }

        if (GUILayout.Button("Release Ghost"))
        {
            hauntableToy.ReleaseGhost();
        }
    }
}