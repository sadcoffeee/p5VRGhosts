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
            mesh.gameObject.layer = OutlineLayer; //& is the layer the outline uses
        }
        else
        {
            mesh.gameObject.layer = DefaultLayer; //= is defeault layer
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
}
