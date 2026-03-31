using UnityEngine;

[RequireComponent(typeof(HauntableToy))]
public class HauntableToyTestScript : MonoBehaviour
{
    //Settings

    //Variables
    HauntableToy hauntableToy;

    //Logic
    private void Awake()
    {
        hauntableToy = GetComponent<HauntableToy>();
    }

    private void OnMouseDown()
    {
        print("test");
        hauntableToy.StealToy();
    }

    //Methods
}
