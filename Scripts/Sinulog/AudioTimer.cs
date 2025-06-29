using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AudioTimer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private FadeScreen fadeScreen; // Reference to the FadeScreen script
    [SerializeField] private string gameMenuSceneName = "Game Menu"; // Name of the Game Menu Scene

    private bool isTransitioning = false; // Prevent multiple transitions
    private ScoreSender scoreSender; // Reference to the ScoreSender component

    void Start()
    {
        // Find the ScoreSender in the scene
        scoreSender = FindObjectOfType<ScoreSender>();
        if (scoreSender == null)
        {
            scoreSender.HandleGameEnd();
        }

        // Start the coroutine to play audio
        StartCoroutine(StartAudioCoroutine());
    }

    private IEnumerator StartAudioCoroutine()
    {
        // Optional: Add a delay before starting the audio
        yield return new WaitForSeconds(1f); // Adjust the delay as needed

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if (audioSource == null || audioSource.clip == null) return;

        // Update the timer and progress bar
        float currentTime = audioSource.time;
        float songLength = audioSource.clip.length;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        int totalMinutes = Mathf.FloorToInt(songLength / 60);
        int totalSeconds = Mathf.FloorToInt(songLength % 60);

        timerText.text = string.Format("{0:00}:{1:00} / {2:00}:{3:00}", minutes, seconds, totalMinutes, totalSeconds);

        progressBar.value = currentTime / songLength;

        // Check if the audio has finished playing
        if (!audioSource.isPlaying && currentTime >= songLength && !isTransitioning)
        {
            StartCoroutine(EndAudioAndTransition());
        }
    }

    // private IEnumerator EndAudioAndTransition()
    // {
    //     isTransitioning = true;

    //     // Save the score before fading out
    //     if (scoreSender != null)
    //     {
    //         // This will save the score in the ScorePersistenceManager
    //         Debug.Log("Saving score before transition");
    //         // We don't call SendScoreAndReturnToMenu() here because we want to handle
    //         // the transition ourselves with the fade effect
    //         ScorePersistenceManager scorePersistenceManager = ScorePersistenceManager.Instance;
    //         if (scorePersistenceManager != null)
    //         {
    //             ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
    //             if (scoreManager != null)
    //             {
    //                 scorePersistenceManager.SaveScore(scoreSender.gameType, scoreManager.totalScore);
    //                 Debug.Log($"Saved score {scoreManager.totalScore} for {scoreSender.gameType}");
    //             }
    //         }
    //     }

    //     // Trigger fade-out
    //     if (fadeScreen != null)
    //     {
    //         fadeScreen.FadeOut();
    //         yield return new WaitForSeconds(fadeScreen.fadeDuration);
    //     }

    //     // Load the Game Menu Scene
    //     SceneManager.LoadScene(gameMenuSceneName);
    // }

    private IEnumerator EndAudioAndTransition()
    {
        isTransitioning = true;

        // Save the score before showing results
        if (scoreSender != null)
        {
            ScorePersistenceManager scorePersistenceManager = ScorePersistenceManager.Instance;
            if (scorePersistenceManager != null)
            {
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    scorePersistenceManager.SaveScore(scoreSender.gameType, scoreManager.totalScore);
                    Debug.Log($"Saved score {scoreManager.totalScore} for {scoreSender.gameType}");
                }
            }
        }

        // Show the performance assessment panel
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ShowPerformanceAssessment();
        }

        // Optionally, you can fade out or wait here, but DO NOT load the menu scene automatically
        // If you want to allow the player to return to menu via a button, do nothing else here

        yield break;
    }
}