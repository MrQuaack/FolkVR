using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TriangleUIBounce : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float bounceDuration = 0.1f; // Duration of the bounce animation
    public Vector3 bounceScale = new Vector3(1.2f, 1.2f, 1.2f); // Scale during the bounce
    private Vector3 originalScale; // Original scale of the button
    private Coroutine bounceCoroutine; // To ensure only one animation runs at a time

    void Start()
    {
        originalScale = transform.localScale; // Store the original scale
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop any ongoing animation
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }

        // Start the bounce animation to scale up
        bounceCoroutine = StartCoroutine(ScaleTo(bounceScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop any ongoing animation
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }

        // Start the animation to scale back to the original size
        bounceCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 currentScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, elapsedTime / bounceDuration);
            yield return null;
        }

        transform.localScale = targetScale; // Ensure the final scale is set
        bounceCoroutine = null;
    }
}