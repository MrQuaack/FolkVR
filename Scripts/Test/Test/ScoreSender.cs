using UnityEngine;
using UnityEngine.SceneManagement;

// Add this script to the ScoreManager in each scene
public class ScoreSender : MonoBehaviour
{
    [Header("Scene Information")]
    [Tooltip("Name of this game scene (Sinulog, Alitaptap, or Maglalatik)")]
    public string gameType = "Sinulog"; // Set this in the Inspector for each scene

    [Tooltip("Name of the menu scene to load when game ends")]
    public string menuSceneName = "GameMenu";

    private ScoreManager scoreManager;

    private void Start()
    {
        // Get reference to the ScoreManager
        scoreManager = GetComponent<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager component not found on this GameObject!");
        }

        // Check if ScorePersistenceManager exists in the scene
        if (ScorePersistenceManager.Instance == null)
        {
            // Create the persistence manager if it doesn't exist
            GameObject persistenceObj = new GameObject("ScorePersistenceManager");
            persistenceObj.AddComponent<ScorePersistenceManager>();
            Debug.Log("Created ScorePersistenceManager as it was not found in the scene");
        }
    }

    public void HandleGameEnd()
    {
        if (scoreManager != null && ScorePersistenceManager.Instance != null)
        {
            // Save the final score
            ScorePersistenceManager.Instance.SaveScore(gameType, scoreManager.totalScore);
            Debug.Log($"Sent score {scoreManager.totalScore} for {gameType} to ScorePersistenceManager");

            // Show performance assessment
            scoreManager.ShowPerformanceAssessment();
        }
        else
        {
            Debug.LogError("Could not send score: ScoreManager or ScorePersistenceManager is missing");
        }
    }

    // Call this method when the game is over
    public void SendScoreAndReturnToMenu()
    {
        if (scoreManager != null && ScorePersistenceManager.Instance != null)
        {
            // Save the final score
            // ScorePersistenceManager.Instance.SaveScore(gameType, scoreManager.totalScore);
            // Debug.Log($"Sent score {scoreManager.totalScore} for {gameType} to ScorePersistenceManager");

            // Load the menu scene
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("Could not send score: ScoreManager or ScorePersistenceManager is missing");
        }
    }
}