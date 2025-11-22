using UnityEngine;

public class GrablinghandAnimations : MonoBehaviour
{

    private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        PlayHandIdle();
    }

    public void PlayHandIdle()
    {
        anim.Play("Idle");
        Debug.Log("Playing idle");
    }

    public void PlayHandReach()
    {
        anim.Play("Reaching");
    }

    public void PlayHandGrab()
    {
        anim.Play("Grabbing");
    }
}
