using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class PossessedObject : MonoBehaviour
{
    private Renderer currentObject;
    public Material endMaterial;
    public bool isPossessed = false;
    private GameObject ghostFace;
    private Material startMaterial;
    public GameObject ghostOccupying;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float rumbleTimer = 0f;
    private float rumbleAmount = 0.05f;
    private float rumbleInterval = 2;
    private float rumbleTime = 0.5f;
    private HapticImpulsePlayer hapticPlayer;

    private void Start()
    {
        currentObject = GetComponent<Renderer>();
        ghostFace = transform.GetChild(0).gameObject;
        ghostFace.SetActive(false);
        startMaterial = GetComponent<Renderer>().material;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        hapticPlayer = GetComponent<HapticImpulsePlayer>();
    }

    private void Update()
    {
        if (isPossessed)
        {
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
            PossessRumble();
        }
    }
    public void SetPossessed(bool value, GameObject ghost = null)
    {
        isPossessed = value;

        if (isPossessed)
        {
            ghostOccupying = ghost;
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
        }
        else
        {
            ghostFace.SetActive(false);
            currentObject.material = startMaterial;
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            //Reactivate ghost and release it from furniture
            ghostOccupying.transform.position = transform.position + transform.forward * 2f;
            ghostOccupying.SetActive(true);
            FlyTowardsGhost ghostScript = ghostOccupying.GetComponent<FlyTowardsGhost>();
            ghostScript.currentState = FlyTowardsGhost.GhostState.Stunned;
            ghostOccupying = null;

        }
    }

    private void PossessRumble()
    {
        rumbleTimer += Time.deltaTime;
        if (rumbleTimer >= rumbleInterval)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-rumbleAmount, rumbleAmount),
                Random.Range(-rumbleAmount, rumbleAmount),
                Random.Range(-rumbleAmount, rumbleAmount)
            );
            transform.localPosition = originalPosition + randomOffset;

            Vector3 randomRotation = new Vector3(
                Random.Range(-rumbleAmount, rumbleAmount),
                Random.Range(-rumbleAmount, rumbleAmount),
                Random.Range(-rumbleAmount, rumbleAmount)
            );
            transform.localRotation = Quaternion.Euler(originalRotation.eulerAngles + randomRotation);
            hapticPlayer.SendHapticImpulse(0.1f, 0.1f);
            if (rumbleTimer >= rumbleTime + rumbleInterval)
            {
                rumbleTimer = 0;
            }
        }

        else
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
        }
    }
}

