using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GhostCapsuleManager : MonoBehaviour
{
    [Header("Mini Ghost Settings")]
    public GameObject miniGhostPrefab;
    public int maxMiniGhosts = 10;

    [Header("Capsule Bounds")]
    public float capsuleRadius = 0.08f;
    public float capsuleHalfHeight = 0.12f;
    public float miniGhostScale = 0.05f;

    [Header("UI")]
    public TMP_Text catchCounterText;

    private readonly List<GameObject> _spawnedGhosts = new();
    private int _totalCaught = 0;

    // Public API
    public void OnGhostCaught()
    {
        _totalCaught++;
        UpdateCounterText();

        if (_spawnedGhosts.Count < maxMiniGhosts)
            SpawnMiniGhost();
        // If cap reached we keep existing ghosts and only update the counter.
    }
    public int TotalCaught() { return _totalCaught; }

    // Internals
    private void SpawnMiniGhost()
    {
        Vector3 localPos = RandomPointInCapsule();

        GameObject mini = Instantiate(miniGhostPrefab, transform.TransformPoint(localPos), Random.rotation, transform);

        mini.transform.localScale = Vector3.one * miniGhostScale;

        // Pass capsule bounds to the movement component
        MiniGhostFloater floater = mini.GetComponent<MiniGhostFloater>();
        if (floater == null)
            floater = mini.AddComponent<MiniGhostFloater>();

        floater.Initialize(capsuleRadius, capsuleHalfHeight, miniGhostScale * 0.5f);

        _spawnedGhosts.Add(mini);
    }

    private Vector3 RandomPointInCapsule()
    {
        // Shrink the usable radius/height so the ghost mesh doesn't clip the wall
        float r = capsuleRadius - miniGhostScale * 0.5f;
        float hh = capsuleHalfHeight; // hemisphere centres are at ±hh on the Y axis

        // Pick a random height along the full capsule height
        float totalHalf = hh + capsuleRadius;
        float y = Random.Range(-totalHalf, totalHalf);

        Vector3 point;
        if (Mathf.Abs(y) <= hh)
        {
            // Cylindrical region
            Vector2 disk = Random.insideUnitCircle * r;
            point = new Vector3(disk.x, y, disk.y);
        }
        else
        {
            // Hemispherical cap — pick inside sphere of radius r, then offset centre
            point = Random.insideUnitSphere * r;
            float sign = y > 0 ? 1f : -1f;
            point.y += sign * hh;
        }

        return point;
    }

    private void UpdateCounterText()
    {
        if (catchCounterText != null)
            catchCounterText.text = _totalCaught.ToString();
    }

#if UNITY_EDITOR
    // Draw capsule gizmo for easy setup in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.4f);
        // Draw two spheres for the caps and a wire cube for the body as a rough preview
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(new Vector3(0,  capsuleHalfHeight, 0), capsuleRadius);
        Gizmos.DrawWireSphere(new Vector3(0, -capsuleHalfHeight, 0), capsuleRadius);
        // Side lines
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * capsuleRadius;
            Gizmos.DrawLine(offset + Vector3.up * capsuleHalfHeight, offset - Vector3.up * capsuleHalfHeight);
        }
    }
#endif
}
