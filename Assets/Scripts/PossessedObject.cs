using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class PossessedObject : MonoBehaviour
{
    private Renderer currentObject;
    public Material endMaterial;
    public Material DeadMaterial;


    public bool isPossessed = false;
    public bool isFurnitureBroken = false;

    private GameObject ghostFace;
    private Material startMaterial;
    public GameObject ghostOccupying;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float rumbleTimer = 0f;
    public Vector3 rumbleBounds = new Vector3(0.05f, 0.05f, 0.05f);
    public Vector3 rotationBounds = new Vector3(2f, 2f, 2f); // degrees
    private float rumbleInterval = 2;
    private float rumbleTime = 0.5f;
    private HapticImpulsePlayer hapticPlayer;

    //HealtBar
    public Canvas healtbarCanvas;
    public Image lifeImage;
    public Image exorcismImage;
    public float exorcismRequiredTime = 3f;
    private float exorcismCounter = 0f;
    private bool isBeingHealed = false;
    public float possessionDuration = 7f;
    private float currentTimer;
    public GameObject Hand;
    public VibratorController vibratorController;
    private GhostAnimations anim;
    private ParticleSystem exorcismParticles;


    private void Start()
    {
        currentObject = GetComponent<Renderer>();
        ghostFace = transform.GetChild(0).gameObject;
        ghostFace.SetActive(false);
        startMaterial = GetComponent<Renderer>().material;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        hapticPlayer = GetComponent<HapticImpulsePlayer>();
        exorcismParticles = GetComponentInChildren<ParticleSystem>();

        //Initialize timer
        currentTimer = possessionDuration;
    }

    private void Update()
    {
        //if the furniture is possesed reduce the timer
        if (isPossessed && GameManager.Instance.tutorialDone)
        {
            //Debug.Log("jeg er possessed");
            healtbarCanvas.gameObject.SetActive(true);

            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
            PossessRumble();

            //Decrease the timer
            if (!isBeingHealed)
            {
                currentTimer -= Time.deltaTime;
            }
            //Convert time left into a value between 0 and 1 for the image fill for UI
            float normalizedTime = currentTimer / possessionDuration;
            lifeImage.fillAmount = normalizedTime;

            if (currentTimer < 0)
            {
                //Debug.Log("i am dead");
                FurnitureDead();
            }
        }
        else if (isPossessed && !GameManager.Instance.tutorialDone)
        {
            healtbarCanvas.gameObject.SetActive(true);
            float normalizedTime = currentTimer / possessionDuration;
            lifeImage.fillAmount = normalizedTime;
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
            PossessRumble();
        }
        
        // Reset healing flag every frame
        isBeingHealed = false;

    }
    public void SetPossessed(bool value, GameObject ghost = null)
    {

        isPossessed = value;

        if (isPossessed)
        {
            StartCoroutine(playNociceptiveHaptics());

            ghostOccupying = ghost;
            currentObject.material = endMaterial;
            ghostFace.SetActive(true);

            // Reset timer to full duration
            currentTimer = possessionDuration;

            // Reset UI fill
            if (lifeImage != null)
                lifeImage.fillAmount = 1f;

            AudioManager.Instance.PlayAudio("PossessFurniture");
        }
        else
        {
            foreach (Transform child in ghost.transform)
            {
                child.gameObject.SetActive(true);
            }

            ghostFace.SetActive(false);

            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            if (!isFurnitureBroken)
            {
                exorcismCounter = 0f;
                isBeingHealed = false;
                exorcismImage.fillAmount = 0f;

                currentObject.material = startMaterial;
                StartCoroutine(ExorciseGhostRoutine(ghostOccupying));
            }
            else
            {
                FlyTowardsGhost ghostScript = ghostOccupying.GetComponent<FlyTowardsGhost>();
                ghostScript.currentState = FlyTowardsGhost.GhostState.DestroyedFurniture;
                ghostOccupying.transform.LookAt(Hand.transform.position - new Vector3(0, 2f, 0));
                ghostOccupying.SetActive(true);
            }

            ghostOccupying = null;

            // Reset everything timer an fill amount
            currentTimer = possessionDuration;
            lifeImage.fillAmount = 1f;
            healtbarCanvas.gameObject.SetActive(false);

            AudioManager.Instance.StopAudio("PossessFurniture");
        }
    }

    private void PossessRumble()
    {
        rumbleTimer += Time.deltaTime;
        if (rumbleTimer >= rumbleInterval)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-rumbleBounds.x, rumbleBounds.x),
                Random.Range(-rumbleBounds.y, rumbleBounds.y),
                Random.Range(-rumbleBounds.z, rumbleBounds.z)
            );

            transform.localPosition = originalPosition + randomOffset;

            Vector3 randomRotation = new Vector3(
                Random.Range(-rotationBounds.x, rotationBounds.x),
                Random.Range(-rotationBounds.y, rotationBounds.y),
                Random.Range(-rotationBounds.z, rotationBounds.z)
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
    private IEnumerator ExorciseGhostRoutine(GameObject ghost)
    {
        FlyTowardsGhost ghostScript = ghost.GetComponent<FlyTowardsGhost>();
        ghostScript.enabled = false;
        ghost.SetActive(true);
        anim = ghost.GetComponent<GhostAnimations>();
        anim.PlayDizzy();
        exorcismParticles.Play();
        AudioManager.Instance.PlayAudio("Exorcism");
        
        float duration = 2.5f; // seconds for the animation
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.up * 2f;

        // spiralling animation
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            ghost.transform.LookAt(Hand.transform);

            // gradually scale up
            ghost.transform.localScale = new Vector3(1 * t, 1 * t, 1 * t);

            // Vertical movement
            float height = Mathf.Lerp(0f, (endPos - startPos).y, t);

            // Spiral parameters
            float radius = 0.5f * (1f - t); // radius shrinks as ghost rises
            float spinSpeed = 1f; // rotations per second
            float angle = elapsed * spinSpeed * Mathf.PI * 2f;

            // Circular offset
            float offsetX = Mathf.Cos(angle) * radius;
            float offsetZ = Mathf.Sin(angle) * radius;

            // Combine everything
            ghost.transform.position = startPos + new Vector3(offsetX, height, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // After animation completes, actually release and stun the ghost
        ghost.transform.position = endPos;
        ghostScript.enabled = true;

        ghostScript.currentState = FlyTowardsGhost.GhostState.Stunned;
        ghostScript.grabbable = true;
        AudioManager.Instance.PlayAudio("GhostStunned");
    }


    private void FurnitureDead()
    {
        currentObject.material = DeadMaterial;
        isFurnitureBroken = true;

        //Debug.Log("jeg er ødelagt");
        GameManager.Instance.UnregisterFurniture(this);
        SetPossessed(false, ghostOccupying);
    }
    public void FurnitureRessurect()
    {
        currentObject.material = startMaterial;
        isFurnitureBroken = false;
    }

    public void HealFurniture()
    {
        if (!isPossessed) return;

        isBeingHealed = true;

        exorcismCounter += Time.deltaTime;
        exorcismCounter = Mathf.Clamp(exorcismCounter, 0f, exorcismRequiredTime);

        if (exorcismImage != null)
        {
            exorcismImage.fillAmount = exorcismCounter / exorcismRequiredTime;
        }

        if (exorcismCounter >= exorcismRequiredTime)
        {
            SetPossessed(false, ghostOccupying);
        }
    }

    public void UpdateBreakTime(float newBreakTime)
    {
        possessionDuration = newBreakTime;
        currentTimer = newBreakTime;
    }

    IEnumerator playNociceptiveHaptics()
    {
        float timer = 0f;

        while (timer < 4.9f)
        {
            timer += Time.deltaTime;
            Debug.Log(timer.ToString());
            vibratorController.SendArduinoSignal("PC", 100);
            vibratorController.SendArduinoSignal("PL", 100);
            yield return null;
        }

        while (isPossessed)
        {
            vibratorController.SendArduinoSignal("PC", 100);
            vibratorController.SendArduinoSignal("PL", 100);
            yield return null;
        }

        yield return null;
    }

}

