using UnityEngine;
using TMPro;

public class GhostCounterUI : MonoBehaviour
{
    public TextMeshProUGUI counterText; // assign your TMP text here

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            int count = GameManager.Instance.GhostsDefeated;
            counterText.text = count.ToString();
        }
    }
}
