using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Method to load a new scene by name
    public void LoadScene(string sceneName)
    {
        StopMusic();

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        // Stop music in the current scene
        StopMusic();

        // Load the new scene
        SceneManager.LoadScene(sceneIndex);
    }

    // Method to stop music in the current scene
    private void StopMusic()
    {
        // Stop the MusicManager's audio source if it exists
        if (MusicManager.Instance != null && MusicManager.Instance.audioSource.isPlaying)
        {
            MusicManager.Instance.audioSource.Stop();
        }
    }

    // Method to handle audio in the new scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the new scene does not require music, do nothing
        if (scene.name == "Scene2")
        {
            return; // Skip resuming music
        }

        // Resume the MusicManager if it exists and the new scene requires music
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.audioSource.Play();
        }
    }

    private void OnEnable()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}