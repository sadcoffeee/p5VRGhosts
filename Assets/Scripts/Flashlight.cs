using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class VRFlashlight : MonoBehaviour
{
    public float range = 100f;
    public float unpossessTime = 3f;
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
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
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
                    if (hoverTimer >= unpossessTime)
                    {
                        possessed.SetPossessed(false);
                        currentTarget = null;
                        hoverTimer = 0f;
                    }
                }
            }

            //Debug.DrawRay(transform.position, transform.forward * range, Color.yellow);
        }
    }
}
