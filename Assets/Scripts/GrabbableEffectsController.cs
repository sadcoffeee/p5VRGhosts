using UnityEngine;

public class GrabbableEffectsController : MonoBehaviour
{
    public float heightOffset = 2f;

    private Transform parent;

    private void Start()
    {
        parent = transform.parent;
        if (parent == null)
        {
            Debug.LogWarning($"{name} has no parent!");
        }
    }

    private void LateUpdate()
    {
        if (parent == null) return;

        transform.position = parent.position + Vector3.up * heightOffset;

        transform.rotation = Quaternion.identity;
    }
}
