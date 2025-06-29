using System.Collections;
using UnityEngine;

public class MenuFadeInBounce : MonoBehaviour
{
    public CanvasGroup menuCanvasGroup; // CanvasGroup for the menu
    public float fadeInDuration = 1.0f; // Duration of the fade-in animation
    public float bounceDuration = 0.5f; // Duration of the bounce animation
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f); // Initial scale for the bounce
    public Vector3 endScale = Vector3.one; // Final scale after the bounce

    void Start()
    {
        // Initialize the menu canvas as invisible and scaled down
        menuCanvasGroup.alpha = 0f;
        transform.localScale = startScale;

        // Start the fade-in bounce animation
        StartCoroutine(FadeInBounce());
    }

    private IEnumerator FadeInBounce()
    {
        // Fade in the canvas
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;

            menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t); // Fade in the alpha
            yield return null;
        }

        menuCanvasGroup.alpha = 1f; // Ensure the alpha is fully visible

        // Bounce animation
        elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / bounceDuration;

            // Smooth bounce effect using Lerp and Mathf.PingPong
            transform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = endScale; // Ensure the scale is fully set
    }
}