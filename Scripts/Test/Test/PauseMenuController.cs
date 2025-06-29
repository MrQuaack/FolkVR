using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Import TextMeshPro namespace
using UnityEngine.InputSystem; // Import the Input System namespace

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuUI; // Assign the pause menu Panel in the Inspector
    public TMP_Text countdownText; // Assign the CountdownText TextMeshPro element in the Inspector
    private AudioSource[] allAudioSources;
    private bool isPaused = false;

    // Input action for the X button
    private InputAction openMenuAction;

    void Awake()
    {
        // Initialize the input action for the X button on the left-hand controller
        openMenuAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{LeftHand}/x");
        openMenuAction.performed += ctx => TogglePauseMenu();
        openMenuAction.Enable();
    }

    void Start()
    {
        // Ensure the countdown text is hidden at the start
        countdownText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // Clean up the input action
        openMenuAction.Disable();
        openMenuAction.Dispose();
    }

    private void TogglePauseMenu()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;

        // Pause all audio sources
        allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in allAudioSources)
        {
            audioSource.Pause();
        }
    }

    public void Resume()
    {
        StartCoroutine(ResumeAfterCountdown(3)); // Start the 3-second countdown
    }

    private System.Collections.IEnumerator ResumeAfterCountdown(int seconds)
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        countdownText.gameObject.SetActive(true); // Show the countdown text

        // Countdown loop
        for (int i = seconds; i > 0; i--)
        {
            countdownText.text = i.ToString(); // Update the countdown text
            yield return new WaitForSecondsRealtime(1f); // Wait for 1 second in real time
        }

        countdownText.text = ""; // Clear the countdown text
        countdownText.gameObject.SetActive(false); // Hide the countdown text

        Time.timeScale = 1f; // Unfreeze the game
        isPaused = false;

        // Resume all audio sources
        foreach (var audioSource in allAudioSources)
        {
            audioSource.UnPause();
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Ensure time is normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f; // Ensure time is normal
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with your menu scene name
    }

    public void ExitGame()
    {
        Time.timeScale = 1f; // Ensure time is normal
        SceneManager.LoadScene(2); // Replace "MainMenu" with the name of your target scene
    }
}