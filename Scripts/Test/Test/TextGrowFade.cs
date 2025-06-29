using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextGrowFade : MonoBehaviour
{
    public float duration = 1.0f; // Duration of the fade-in animation
    public float fadeOutDuration = 1.0f; // Duration of the fade-out animation
    public float waitBeforeFadeOut = 3.0f; // Wait time before fading out
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f); // Initial scale
    public Vector3 endScale = Vector3.one; // Final scale
    public Color startColor = new Color(1, 1, 1, 0); // Transparent color
    public Color endColor = new Color(1, 1, 1, 1); // Fully visible color

    public CanvasGroup planeCanvasGroup; // CanvasGroup for the fade screen plane
    public Image image1; // First element (Image)

    public AudioSource audioSource; // AudioSource to play music
    public AudioClip[] musicTracks; // Array of music tracks
    public bool loopMusic = true; // Whether to loop the music

    private bool isFadingOut = false;

    void Start()
    {
        // Initialize the image
        image1.color = startColor;
        image1.transform.localScale = startScale;

        // Start the fade-in sequence
        StartCoroutine(FadeInElements());
    }

    private IEnumerator FadeInElements()
    {
        // Fade in the image
        yield return StartCoroutine(FadeInImage(image1));

        // Wait before starting the fade-out
        yield return new WaitForSeconds(waitBeforeFadeOut);

        // Start fade-out sequence
        StartCoroutine(FadeOutElementsAndPlane());
    }

    private IEnumerator FadeInImage(Image image)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Interpolate scale and color
            image.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            image.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
    }

    private IEnumerator FadeOutElementsAndPlane()
    {
        // Fade out the image
        float elapsedTime = 0f;
        Color image1Color = image1.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;

            image1.color = new Color(image1Color.r, image1Color.g, image1Color.b, Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        // Fade out the plane
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            planeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        planeCanvasGroup.gameObject.SetActive(false); // Hide the plane after fading out

        // Play music after fade-out
        PlayMusic();
    }

    private void PlayMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayPlaylist(); // Start the playlist
        }
        else
        {
            Debug.LogWarning("MusicManager instance not found!");
        }
    }
}