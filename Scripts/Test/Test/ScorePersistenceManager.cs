using UnityEngine;
using System.Collections.Generic;

public class ScorePersistenceManager : MonoBehaviour
{
    // Singleton instance
    public static ScorePersistenceManager Instance;

    // Constants for PlayerPrefs keys
    private const string SINULOG_SCORES_KEY = "SinulogScores";
    private const string SINULOG_EASY_SCORES_KEY = "SinulogEasyScores";
    private const string ALITAPTAP_SCORES_KEY = "AlitaptapScores";
    private const string ALITAPTAP_EASY_SCORES_KEY = "AlitaptapEasyScores";
    private const string MAGLALATIK_SCORES_KEY = "MaglalatikScores";
    private const string MAGLALATIK_EASY_SCORES_KEY = "MaglalatikEasyScores";
    private const int MAX_SCORES_TO_SAVE = 5; // Number of top scores to save

    // Store the current player name for this session
    public static string CurrentPlayerName = "";

    // Structure to hold score data
    [System.Serializable]
    public class ScoreData
    {
        public int score;
        public string playerName;
        public string date;

        public ScoreData(int score, string playerName)
        {
            this.score = score;
            this.playerName = playerName;
            this.date = System.DateTime.Now.ToString("dd/MM/yyyy");
        }
    }

    private void Awake()
    {
        // Ensure this object persists between scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ScorePersistenceManager: New instance created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log($"ScorePersistenceManager: Duplicate instance destroyed. Current player name: {CurrentPlayerName}");
            Destroy(gameObject);
        }
    }

    // Set the current player name (called from name input system)
    public void SetCurrentPlayerName(string name)
    {
        CurrentPlayerName = name;
        Debug.Log($"ScorePersistenceManager: Player name set to: '{CurrentPlayerName}'");
    }

    // Get the current player name
    public string GetCurrentPlayerName()
    {
        Debug.Log($"ScorePersistenceManager: Getting player name: '{CurrentPlayerName}'");
        return CurrentPlayerName;
    }

    // Call this when a game ends to save the score
    public void SaveScore(string gameType, int score)
    {
        string key = GetKeyForGameType(gameType);
        List<ScoreData> scores = LoadScores(gameType);
        
        // Use current player name, or "Anonymous" if not set
        string playerName = string.IsNullOrEmpty(CurrentPlayerName) ? "Anonymous" : CurrentPlayerName;
        
        // Add new score
        scores.Add(new ScoreData(score, playerName));
        
        // Sort in descending order
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Trim to max scores
        if (scores.Count > MAX_SCORES_TO_SAVE)
        {
            scores.RemoveRange(MAX_SCORES_TO_SAVE, scores.Count - MAX_SCORES_TO_SAVE);
        }
        
        // Save back to PlayerPrefs
        string json = JsonUtility.ToJson(new ScoreDataList { scores = scores });
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved score {score} for {playerName} in {gameType}. Total scores: {scores.Count}");
    }

    // Get all saved scores for a game type
    public List<ScoreData> LoadScores(string gameType)
    {
        string key = GetKeyForGameType(gameType);
        string json = PlayerPrefs.GetString(key, "");
        
        if (string.IsNullOrEmpty(json))
        {
            return new List<ScoreData>();
        }
        
        ScoreDataList scoreDataList = JsonUtility.FromJson<ScoreDataList>(json);
        return scoreDataList != null ? scoreDataList.scores : new List<ScoreData>();
    }

    // Delete all scores for a specific game type
    public void DeleteScores(string gameType)
    {
        string key = GetKeyForGameType(gameType);
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log($"Deleted all scores for {gameType}");
    }
    
    // Delete all scores for all game types
    public void DeleteAllScores()
    {
        PlayerPrefs.DeleteKey(SINULOG_SCORES_KEY);
        PlayerPrefs.DeleteKey(SINULOG_EASY_SCORES_KEY);
        PlayerPrefs.DeleteKey(ALITAPTAP_SCORES_KEY);
        PlayerPrefs.DeleteKey(ALITAPTAP_EASY_SCORES_KEY);
        PlayerPrefs.DeleteKey(MAGLALATIK_SCORES_KEY);
        PlayerPrefs.DeleteKey(MAGLALATIK_EASY_SCORES_KEY);
        PlayerPrefs.Save();
        Debug.Log("Deleted all scores for all game types");
    }

    // Clear the current player name (call when exiting/quitting)
    public void ClearCurrentPlayerName()
    {
        CurrentPlayerName = "";
        Debug.Log("Player name cleared");
    }

    // Helper method to convert game type to the appropriate key
    private string GetKeyForGameType(string gameType)
    {
        switch (gameType.ToLower())
        {
            case "sinulog":
                return SINULOG_SCORES_KEY;
            case "sinulog_easy":
                return SINULOG_EASY_SCORES_KEY;
            case "alitaptap":
                return ALITAPTAP_SCORES_KEY;
            case "alitaptap_easy":
                return ALITAPTAP_EASY_SCORES_KEY;
            case "maglalatik":
                return MAGLALATIK_SCORES_KEY;
            case "maglalatik_easy":
                return MAGLALATIK_EASY_SCORES_KEY;
            default:
                Debug.LogError($"Unknown game type: {gameType}");
                return "UnknownGame";
        }
    }

    // Helper class for JSON serialization (Unity requires this wrapper for List serialization)
    [System.Serializable]
    private class ScoreDataList
    {
        public List<ScoreData> scores = new List<ScoreData>();
    }
}