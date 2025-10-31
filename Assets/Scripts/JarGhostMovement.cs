using UnityEngine;

public class JarGhostMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 0.2f;

    private Vector3 minBounds;
    private Vector3 maxBounds;
    private Vector3 targetPos;
    void Start()
    {
        // Get the corners from the GameManager
        if (GameManager.Instance != null && GameManager.Instance.jarCorners.Length >= 8)
        {
            Transform[] corners = GameManager.Instance.jarCorners;

            // Find min/max bounds from GameManager's corners
            minBounds = corners[0].position;
            maxBounds = corners[0].position;

            foreach (Transform c in corners)
            {
                minBounds = Vector3.Min(minBounds, c.position);
                maxBounds = Vector3.Max(maxBounds, c.position);
            }

            PickNewTarget();
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            {
                PickNewTarget();
            }
        }
    }

    void PickNewTarget()
    {
        float x = Random.Range(minBounds.x, maxBounds.x);
        float y = Random.Range(minBounds.y, maxBounds.y);
        float z = Random.Range(minBounds.z, maxBounds.z);
        targetPos = new Vector3(x, y, z);
    }
}
