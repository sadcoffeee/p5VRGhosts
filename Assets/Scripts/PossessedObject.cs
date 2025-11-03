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
    private float rumbleAmount = 0.05f;
    private float rumbleInterval = 2;
    private float rumbleTime = 0.5f;
    private HapticImpulsePlayer hapticPlayer;

    //HealtBar
    public Canvas healtbarCanvas;
    public Image lifeImage;
    public float possessionDuration = 5f;
    private float currentTimer;
    public GameObject Hand;


    private void Start()
    {
        currentObject = GetComponent<Renderer>();
        ghostFace = transform.GetChild(0).gameObject;
        ghostFace.SetActive(false);
        startMaterial = GetComponent<Renderer>().material;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        hapticPlayer = GetComponent<HapticImpulsePlayer>();

        //Initialize timer
        currentTimer = possessionDuration;
    }

    private void Update()
    {
        //if the furniture is possesed reduce the timer
        if (isPossessed)
        {
            //Debug.Log("jeg er possessed");
            healtbarCanvas.gameObject.SetActive(true);

            currentObject.material = endMaterial;
            ghostFace.SetActive(true);
            PossessRumble();

            //Decrease the timer
            currentTimer -= Time.deltaTime;
            //Convert time left into a value between 0 and 1 for the image fill for UI
            float normalizedTime = currentTimer / possessionDuration;
            lifeImage.fillAmount = normalizedTime;

            if (currentTimer < 0)
            {
                //Debug.Log("i am dead");
                FurnitureDead();
            }
            
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

            // Reset timer to full duration
            currentTimer = possessionDuration;

            // Reset UI fill
            if (lifeImage != null)
                lifeImage.fillAmount = 1f;

            FindObjectOfType<AudioManager>().PlayAudio("PossessFurniture");
        }
        else
        { 
            ghostFace.SetActive(false);
            if (!isFurnitureBroken)
            {
                currentObject.material = startMaterial;
            }
            
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            //Reactivate ghost and release it from furniture
            ghostOccupying.transform.position = transform.position + transform.up * 2f;
            ghostOccupying.transform.LookAt(Hand.transform);
            ghostOccupying.SetActive(true);
            FlyTowardsGhost ghostScript = ghostOccupying.GetComponent<FlyTowardsGhost>();
            if(!isFurnitureBroken)
            {
                currentObject.material = startMaterial;
                ghostScript.currentState = FlyTowardsGhost.GhostState.Stunned;
            }
            else
            {
                ghostScript.currentState = FlyTowardsGhost.GhostState.Hovering;
            }

                ghostOccupying = null;

            // Reset everything timer an fill amount
            currentTimer = possessionDuration;
            lifeImage.fillAmount = 1f;
            healtbarCanvas.gameObject.SetActive(false);

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

    private void FurnitureDead()
    {
        currentObject.material = DeadMaterial;
        isFurnitureBroken = true;
        
        //Debug.Log("jeg er ødelagt");

        SetPossessed(false, ghostOccupying);


    }

}

