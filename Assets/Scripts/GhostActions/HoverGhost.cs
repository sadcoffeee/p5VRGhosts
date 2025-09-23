using UnityEngine;

public class HoverGhost : MonoBehaviour
{
    public float speed = 2f;
    public float height = 0.5f;

    public Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * height;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
