using UnityEngine;

public class TutorialVRControllerImage : MonoBehaviour
{
    private MeshRenderer controllerMat;
    public Texture2D[] images;
    private float timer;
    private int currentIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controllerMat = GetComponent<MeshRenderer>();
        controllerMat.material.mainTexture = images[currentIndex];

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % images.Length;
            controllerMat.material.mainTexture = images[currentIndex];
        }

    }
}
