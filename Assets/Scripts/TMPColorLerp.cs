using UnityEngine;
using TMPro;

public class TMPColorLerp : MonoBehaviour
{
    [Header("References")]
    public TMP_Text tmpText; // Assign in Inspector or auto-detect

    [Header("Color Settings")]
    public Color startColor = Color.white;
    public Color endColor = Color.clear; // e.g., transparent white
    public float lerpDuration = 2f;      // seconds to complete transition

    [Header("Behavior")]
    public bool loop = false;            // if true, goes back and forth

    private float lerpTimer = 0f;
    private bool reversing = false;

    void Start()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        tmpText.color = startColor;
    }

    void Update()
    {
        if (tmpText == null) return;

        // Update timer
        lerpTimer += Time.deltaTime / lerpDuration;

        // Calculate color
        tmpText.color = Color.Lerp(startColor, endColor, lerpTimer);

        // Looping behavior
        if (lerpTimer >= 1f)
        {
            if (loop)
            {
                lerpTimer = 0f;

                // Swap colors for reverse effect
                Color temp = startColor;
                startColor = endColor;
                endColor = temp;
            }
            else
            {
                // Stop at the end color if not looping
                tmpText.color = endColor;
                enabled = false;
            }
        }
    }
}
