using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI[] scoreTexts;
    public TextMeshProUGUI[] nameTexts;  // New: Array for player names
    public TextMeshProUGUI[] dateTexts;
    public TextMeshProUGUI noScoresText;

    [Header("Game Type Buttons")]
    public GameObject sinulogEasyButton;
    public GameObject sinulogButton;
    public GameObject alitaptapEasyButton;
    public GameObject alitaptapButton;
    public GameObject maglalatikEasyButton;
    public GameObject maglalatikButton;

    [Header("Reset Buttons")]
    public Button resetCurrentButton;      // Resets only the current game type scores
    public Button resetAllButton;          // Resets all game scores

    // Current displayed game type
    private string currentGameType = "Sinulog";

    private void Start()
    {
        // Set up reset buttons
        if (resetCurrentButton != null)
        {
            resetCurrentButton.onClick.AddListener(ResetCurrentScores);
        }

        if (resetAllButton != null)
        {
            resetAllButton.onClick.AddListener(ResetAllScores);
        }

        // Show default leaderboard on start
        ShowLeaderboard("Sinulog");
    }

    // Called by UI buttons to change the displayed leaderboard
    public void ShowLeaderboard(string gameType)
    {
        currentGameType = gameType;
        UpdateLeaderboardUI();

        // Update title
        titleText.text = "Leaderboard";
    }

    private void UpdateLeaderboardUI()
    {
        // Get scores for current game type
        List<ScorePersistenceManager.ScoreData> scores = null;

        // Check if we have the persistence manager
        if (ScorePersistenceManager.Instance != null)
        {
            scores = ScorePersistenceManager.Instance.LoadScores(currentGameType);
        }
        else
        {
            Debug.LogError("ScorePersistenceManager instance not found!");
            scores = new List<ScorePersistenceManager.ScoreData>();
        }

        // Show or hide the "no scores" message
        noScoresText.gameObject.SetActive(scores.Count == 0);

        // Update each score entry
        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (i < scores.Count)
            {
                // We have a score to display
                scoreTexts[i].gameObject.SetActive(true);
                nameTexts[i].gameObject.SetActive(true);
                dateTexts[i].gameObject.SetActive(true);

                // Display rank and score
                scoreTexts[i].text = (i + 1) + ". " + scores[i].score.ToString();
                
                // Display player name
                nameTexts[i].text = scores[i].playerName;
                
                // Display date
                dateTexts[i].text = scores[i].date;
            }
            else
            {
                // No more scores to display
                scoreTexts[i].gameObject.SetActive(false);
                nameTexts[i].gameObject.SetActive(false);
                dateTexts[i].gameObject.SetActive(false);
            }
        }
    }

    // Reset scores for the current game type
    public void ResetCurrentScores()
    {
        if (ScorePersistenceManager.Instance != null)
        {
            ScorePersistenceManager.Instance.DeleteScores(currentGameType);
            UpdateLeaderboardUI();

            Debug.Log($"Reset scores for {currentGameType}");
        }
    }

    // Reset all scores for all game types
    public void ResetAllScores()
    {
        if (ScorePersistenceManager.Instance != null)
        {
            ScorePersistenceManager.Instance.DeleteAllScores();
            UpdateLeaderboardUI();

            Debug.Log("Reset all scores");
        }
    }

    // Button handlers
    public void OnSinulogEasyButtonClick()
    {
        ShowLeaderboard("Sinulog_Easy");
    }
    public void OnSinulogButtonClick()
    {
        ShowLeaderboard("Sinulog");
    }
    public void OnAlitaptapEasyButtonClick()
    {
        ShowLeaderboard("Alitaptap_Easy");
    }
    public void OnAlitaptapButtonClick()
    {
        ShowLeaderboard("Alitaptap");
    }
    public void OnMaglalatikEasyButtonClick()
    {
        ShowLeaderboard("Maglalatik_Easy");
    }
    public void OnMaglalatikButtonClick()
    {
        ShowLeaderboard("Maglalatik");
    }
}   