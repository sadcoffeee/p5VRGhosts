using UnityEngine;

public class HoverGhost : MonoBehaviour
{
    public float speed = 2f;
    public float height = 0.5f;
    float startOffset = 0f;

    public Vector3 startPosition;
    float returnspeed = 2f;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed + startOffset) * height;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnEnable()
    {


        if (height <= 0)
        {
            startOffset = 0f;
            return;
        }

        float currY = transform.position.y;
        float normalized = Mathf.Clamp((currY - startPosition.y) / height, -1f, 1f);
        startOffset = (Mathf.Asin(normalized) - Time.time * speed);
    }

    
}
