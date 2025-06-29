using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Scoring Parameters")]
    public float maxDistance = 1f;         // Beyond this, NO points
    public int maxScore = 100;             // Maximum score per wall

    [Header("Perfect Hit Score Multipliers")]
    public float perfectHitMultiplier = 1.0f;     // Score multiplier for perfect hits (distance <= 0.2f)
    public float goodHitMultiplier = 0.8f;        // Score multiplier for good hits (distance <= 0.5f)
    public float okHitMultiplier = 0.5f;          // Score multiplier for ok hits (distance <= 0.8f)

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hitFeedbackText;

    [Header("Performance Assessment UI")]
    public GameObject performancePanel;           // Panel that shows at game end
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI performanceGradeText;
    public TextMeshProUGUI detailedStatsText;
    public Button returnToMenuButton;

    [Header("Step Feedback UI")]
    public TextMeshProUGUI stepFeedbackText;     // Shows "Correct Step!" or "Wrong Step!"
    public Image stepFeedbackBackground;         // Background color for feedback
    public Color correctStepColor = Color.green;
    public Color wrongStepColor = Color.red;
    public float stepFeedbackDuration = 1.5f;

    // Game statistics
    [HideInInspector]
    public int totalScore = 0;

    // Performance tracking
    private int totalWalls = 0;
    private int wallsHit = 0;
    private int perfectHits = 0;
    private int goodHits = 0;
    private int okHits = 0;
    private int poorHits = 0;
    private int missedHits = 0;
    private int cohesionBonuses = 0;
    private float totalAccuracy = 0f;

    private int perfectWalls = 0;
    private int goodWalls = 0;
    private int missedWalls = 0;
    private List<int> wallScores = new List<int>();

    private int currentWallScore = 0;
    private Dictionary<string, bool> bodyPartsHit = new Dictionary<string, bool>();

    // Track walls to avoid double scoring
    private HashSet<int> processedWallIds = new HashSet<int>();

    // Cache hit data until all bodyparts report their collisions with the wall
    private class WallHitData
    {
        public GameObject wall;
        public Dictionary<string, HitData> bodyPartHits = new Dictionary<string, HitData>();
        public bool isCorrectStep = true; // Track if this wall represents a correct dance step
    }

    private class HitData
    {
        public Vector3 hitPosition;
        public Transform closestHitPoint;
        public float distance;
        public string hitPointType;
    }

    // Track active wall collisions
    private Dictionary<int, WallHitData> activeWallHits = new Dictionary<int, WallHitData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize the bodyPartsHit dictionary with all body parts
        foreach (string part in HitDetection.AllBodyParts)
        {
            bodyPartsHit[part] = false;
        }

        // Hide performance panel at start
        if (performancePanel != null)
            performancePanel.SetActive(false);
    }

    // Called by HitDetection script when a body part hits a wall
    public void RegisterBodyPartHit(GameObject wall, string bodyPart, Vector3 hitPosition, Transform closestHitPoint, string hitPointType)
    {
        int wallId = wall.GetInstanceID();

        // If this is a new wall, create a new entry
        if (!activeWallHits.ContainsKey(wallId))
        {
            activeWallHits[wallId] = new WallHitData { wall = wall };
            totalWalls++;
        }

        // Calculate distance between hit position and the target point
        float distance = Vector3.Distance(hitPosition, closestHitPoint.position);

        // Store hit data for this body part
        activeWallHits[wallId].bodyPartHits[bodyPart] = new HitData
        {
            hitPosition = hitPosition,
            closestHitPoint = closestHitPoint,
            distance = distance,
            hitPointType = hitPointType
        };

        // Check if we've collected hits for all body parts
        CheckAndProcessWallHits(wallId);
    }

    // Public method to register a missed wall (for when player doesn't hit it in time)
    public void RegisterMissedWall(GameObject wall)
    {
        totalWalls++;
        missedHits += HitDetection.AllBodyParts.Count; // Count all body parts as missed

        // Show wrong step feedback
        StartCoroutine(ShowStepFeedback(false));

        Debug.Log($"Wall missed! Total walls: {totalWalls}, Missed hits: {missedHits}");
    }

    // Check if we have hits for all body parts and process scoring
    private void CheckAndProcessWallHits(int wallId)
    {
        if (!activeWallHits.ContainsKey(wallId))
            return;

        WallHitData wallData = activeWallHits[wallId];

        // Log current hit status for debugging
        string hitStatus = "Current hit status for wall " + wallId + ": ";
        foreach (string part in HitDetection.AllBodyParts)
        {
            hitStatus += part + ": " + (wallData.bodyPartHits.ContainsKey(part) ? "Hit" : "Not Hit") + ", ";
        }
        Debug.Log(hitStatus);

        // Check if all body parts have registered a hit
        bool allBodyPartsHit = true;
        foreach (string part in HitDetection.AllBodyParts)
        {
            if (!wallData.bodyPartHits.ContainsKey(part))
            {
                allBodyPartsHit = false;
                break;
            }
        }

        if (allBodyPartsHit)
        {
            wallsHit++;

            // Process the wall hit and calculate score
            CalculateWallScore(wallId);

            // Destroy the wall
            Destroy(wallData.wall);

            // Remove the wall from tracking
            activeWallHits.Remove(wallId);
        }
    }

    // Calculate the score for a wall after all body parts have hit it
    private void CalculateWallScore(int wallId)
    {
        if (processedWallIds.Contains(wallId) || !activeWallHits.ContainsKey(wallId))
            return;

        WallHitData wallData = activeWallHits[wallId];
        currentWallScore = 0;

        // Reset the body parts hit dictionary
        foreach (string part in HitDetection.AllBodyParts)
        {
            bodyPartsHit[part] = false;
        }

        // Log hit quality details for each body part
        Debug.Log("Wall " + wallId + " Hit Quality Details:");

        float wallAccuracy = 0f;
        int hitCount = 0;
        bool isCorrectStep = true; // Assume correct unless proven otherwise
        string lowestQuality = "PERFECT";

        // Process each body part hit
        foreach (var entry in wallData.bodyPartHits)
        {
            string bodyPart = entry.Key;
            HitData hitData = entry.Value;

            // Mark this body part as hit
            bodyPartsHit[bodyPart] = true;

            // Calculate hit quality and partial score
            float hitQuality = 1f - Mathf.Clamp01(hitData.distance / maxDistance);
            int partScore = 0;
            string qualityText = "";

            if (hitData.distance <= 0.2f)
            {
                qualityText = "PERFECT";
                partScore = Mathf.RoundToInt(maxScore * perfectHitMultiplier * (1f / HitDetection.AllBodyParts.Count));
                perfectHits++;
            }
            else if (hitData.distance <= 0.5f)
            {
                qualityText = "GOOD";
                partScore = Mathf.RoundToInt(maxScore * goodHitMultiplier * (1f / HitDetection.AllBodyParts.Count));
                goodHits++;
                if (lowestQuality != "MISSED") lowestQuality = "GOOD";
            }
            else
            {
                qualityText = "MISSED";
                partScore = 0;
                missedHits++;
                isCorrectStep = false; // Misses indicate wrong step execution
                lowestQuality = "MISSED";
            }

            // Add to the current wall score
            currentWallScore += partScore;

            // Track for average accuracy calculation
            wallAccuracy += hitQuality;
            hitCount++;

            Debug.Log($"{bodyPart} hit {hitData.hitPointType} target - Distance: {hitData.distance:F2} - Quality: {qualityText} - Score: {partScore}");
        }

        if (lowestQuality == "MISSED")
        {
            currentWallScore = 0;
            missedWalls++;
        }
        else if (lowestQuality == "GOOD")
        {
            goodWalls++;
        }
        else if (lowestQuality == "PERFECT")
        {
            perfectWalls++;
        }


        // Calculate average accuracy for this wall
        if (hitCount > 0)
        {
            wallAccuracy /= hitCount;
            totalAccuracy += wallAccuracy;
        }

        // Check target cohesion for this wall
        bool allBodyPartsTargetsMatch = true;
        string firstTargetType = null;

        foreach (var entry in wallData.bodyPartHits)
        {
            if (firstTargetType == null)
            {
                firstTargetType = entry.Value.hitPointType;
            }
            else if (entry.Value.hitPointType != firstTargetType)
            {
                allBodyPartsTargetsMatch = false;
                break;
            }
        }

        // Add cohesion bonus if all body parts hit the same type of target
        if (allBodyPartsTargetsMatch && currentWallScore > 0)
        {
            int cohesionBonus = Mathf.RoundToInt(maxScore * 0.2f);
            currentWallScore += cohesionBonus;
            cohesionBonuses++;
            Debug.Log($"COHESION BONUS: +{cohesionBonus} (All body parts hit {firstTargetType} targets)");
        }
        else
        {
            isCorrectStep = false; // Inconsistent targets indicate wrong step execution
        }

        // Store this wall's score for performance analysis
        wallScores.Add(currentWallScore);

        // Add the current wall score to the total
        totalScore += currentWallScore;

        // Mark this wall as processed
        processedWallIds.Add(wallId);

        // Show step feedback
        StartCoroutine(ShowStepFeedback(isCorrectStep));

        // Update UI
        UpdateUI(wallAccuracy, allBodyPartsTargetsMatch, lowestQuality);

        Debug.Log($"🎯 WALL SCORE: {currentWallScore} | TOTAL SCORE: {totalScore}");
    }

    // Show feedback for correct/wrong steps
    private IEnumerator ShowStepFeedback(bool isCorrect)
    {
        if (stepFeedbackText == null) yield break;

        // Set feedback text and color
        if (isCorrect)
        {
            stepFeedbackText.text = "Correct Step!";
            stepFeedbackText.color = correctStepColor;
            if (stepFeedbackBackground != null)
                stepFeedbackBackground.color = new Color(correctStepColor.r, correctStepColor.g, correctStepColor.b, 0.3f);
        }
        else
        {
            stepFeedbackText.text = "Wrong Step!";
            stepFeedbackText.color = wrongStepColor;
            if (stepFeedbackBackground != null)
                stepFeedbackBackground.color = new Color(wrongStepColor.r, wrongStepColor.g, wrongStepColor.b, 0.3f);
        }

        // Show the feedback
        stepFeedbackText.gameObject.SetActive(true);
        if (stepFeedbackBackground != null)
            stepFeedbackBackground.gameObject.SetActive(true);

        // Wait for duration
        yield return new WaitForSeconds(stepFeedbackDuration);

        // Hide the feedback
        stepFeedbackText.gameObject.SetActive(false);
        if (stepFeedbackBackground != null)
            stepFeedbackBackground.gameObject.SetActive(false);
    }

    // private void UpdateUI(float accuracy, bool perfectCohesion)
    // {
    //     if (scoreText != null)
    //     {
    //         scoreText.text = "Score: " + totalScore;
    //     }

    //     if (hitFeedbackText != null)
    //     {
    //         string quality = "";
    //         if (accuracy > 0.95f)
    //             quality = "PERFECT!";
    //         else if (accuracy > 0.3f)
    //             quality = "GOOD!";
    //         else
    //             quality = "MISSED";

    //         hitFeedbackText.text = quality + "\n+" + currentWallScore;

    //         // Optional: Animate the feedback text
    //         Invoke("ClearFeedbackText", 2f);
    //     }
    // }

    private void UpdateUI(float accuracy, bool perfectCohesion, string lowestQuality)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }

        if (hitFeedbackText != null)
        {
            string quality = lowestQuality.ToUpper() + "!";
            hitFeedbackText.text = quality + "\n+" + currentWallScore;
            Invoke("ClearFeedbackText", 2f);
        }
    }

    private void ClearFeedbackText()
    {
        if (hitFeedbackText != null)
        {
            hitFeedbackText.text = "";
        }
    }

    // Call this when the game ends to show performance assessment
    public void ShowPerformanceAssessment()
    {
        if (performancePanel == null) return;

        // Calculate performance metrics
        float overallAccuracy = totalWalls > 0 ? totalAccuracy / wallsHit : 0f;
        int totalHitAttempts = perfectHits + goodHits + okHits + poorHits + missedHits;

        // Calculate performance grade
        string grade = CalculatePerformanceGrade(overallAccuracy, wallsHit, totalWalls);

        // Update UI elements
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {totalScore}";

        if (performanceGradeText != null)
            performanceGradeText.text = $"{grade}";

        if (detailedStatsText != null)
        {
            string stats = $"Performance Summary:\n\n" +
                  $"Walls Hit: {wallsHit}/{totalWalls}\n" +
                  $"Perfect Walls: {perfectWalls}\n" +
                  $"Good Walls: {goodWalls}\n" +
                  $"Missed Walls: {missedWalls}\n\n" +
                  $"Overall Accuracy: {(overallAccuracy * 100f):F1}%\n\n" +
                  $"Cohesion Bonuses: {cohesionBonuses}\n" +
                  $"Average Score per Wall: {(wallScores.Count > 0 ? wallScores.Average() : 0):F1}";

            detailedStatsText.text = stats;
        }

        // Show the performance panel
        Debug.Log("Enabling performance panel!");
        performancePanel.SetActive(true);

        // Setup return to menu button if it exists
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(() =>
            {
                // Try to find and use ScoreSender if it exists
                ScoreSender scoreSender = GetComponent<ScoreSender>();
                if (scoreSender != null)
                {
                    scoreSender.SendScoreAndReturnToMenu();
                }
                else
                {
                    Debug.LogWarning("ScoreSender component not found. Add it to use return to menu functionality.");
                }
            });
        }
    }

    private string CalculatePerformanceGrade(float accuracy, int wallsHit, int totalWalls)
    {
        float completionRate = totalWalls > 0 ? (float)wallsHit / totalWalls : 0f;
        float overallPerformance = (accuracy * 0.6f) + (completionRate * 0.4f);

        if (overallPerformance >= 0.95f)
            return "S+ (Master Dancer)";
        else if (overallPerformance >= 0.9f)
            return "S (Expert Dancer)";
        else if (overallPerformance >= 0.8f)
            return "A (Great Dancer)";
        else if (overallPerformance >= 0.7f)
            return "B (Good Dancer)";
        else if (overallPerformance >= 0.6f)
            return "C (Average Dancer)";
        else if (overallPerformance >= 0.5f)
            return "D (Needs Practice)";
        else
            return "F (Needs Practice)";
    }
    

    public void ForceProcessWall(int wallId)
    {
        if (activeWallHits.ContainsKey(wallId))
        {
            CalculateWallScore(wallId);
            Destroy(activeWallHits[wallId].wall);
            activeWallHits.Remove(wallId);
        }
    }

    // Reset all tracking and scoring
    public void ResetScoring()
    {
        totalScore = 0;
        currentWallScore = 0;
        processedWallIds.Clear();
        activeWallHits.Clear();

        // Reset performance tracking
        totalWalls = 0;
        wallsHit = 0;
        perfectHits = 0;
        goodHits = 0;
        okHits = 0;
        poorHits = 0;
        missedHits = 0;
        cohesionBonuses = 0;
        totalAccuracy = 0f;
        wallScores.Clear();

        foreach (string part in HitDetection.AllBodyParts)
        {
            bodyPartsHit[part] = false;
        }

        UpdateScoreUI();

        // Hide performance panel
        if (performancePanel != null)
            performancePanel.SetActive(false);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + totalScore;
        }

        if (hitFeedbackText != null)
        {
            hitFeedbackText.text = "";
        }
    }
}

// Extension method to calculate average (since it's not available in older Unity versions)
public static class ListExtensions
{
    public static float Average(this List<int> list)
    {
        if (list.Count == 0) return 0f;

        float sum = 0f;
        foreach (int value in list)
        {
            sum += value;
        }
        return sum / list.Count;
    }
}