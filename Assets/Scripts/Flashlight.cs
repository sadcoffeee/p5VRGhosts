using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class VRFlashlight : MonoBehaviour
{
    public float range = 100f;
    public float unpossessTime = 3f;
    public float sphereRadius = 0.4f; //added
    public Light flashlight;
    private PossessedObject currentTarget;
    private float hoverTimer = 0f;
    private HapticImpulsePlayer hapticPlayer;

    private void Start()
    {
        hapticPlayer = GetComponent<HapticImpulsePlayer>();
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll (ray, sphereRadius, range); //returns an array of hits

        if (hits.Length > 0) //checks if there are any hits at all
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Furniture"))
                {
                    PossessedObject possessed = hit.collider.GetComponent<PossessedObject>();
                    //Debug.Log(possessed.name);
                    //Debug.Log(hoverTimer);

                    if (currentTarget != possessed)
                    {
                        currentTarget = possessed;
                        hoverTimer = 0f;
                    }
                    if (possessed.isPossessed)
                    {
                        hoverTimer += Time.deltaTime;
                        hapticPlayer.SendHapticImpulse(0.1f, 0.1f);

                        // added Time.deltaTime to counteract furniture health loss, now 1.5 is the amount healed per second
                        possessed.HealFurniture(Time.deltaTime * 1.5f + Time.deltaTime); //added
                    }
                }

                Debug.DrawRay(transform.position, transform.forward * range, Color.gray);
            }
        }
        
    }
}
