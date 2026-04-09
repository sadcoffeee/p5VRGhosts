using UnityEngine;

public class OutlineObject : MonoBehaviour
{
    public MeshRenderer mesh;

    public static OutlineObject selectedObject;

    public int OutlineLayer = 6;
    public int DefaultLayer = 0;

    private void Update()
    {
        if (selectedObject == this)
        {
            mesh.gameObject.layer = OutlineLayer;
        }
        else
        {
            mesh.gameObject.layer = DefaultLayer;
        }
    }

    public void Select()
    {
        selectedObject = this;
    }

    public static void Deselect()
    {
        selectedObject = null;
    }

    private void OnDisable()
    {
        if (selectedObject == this)
        {
            Deselect();
            mesh.gameObject.layer = DefaultLayer;
        }
    }
}
