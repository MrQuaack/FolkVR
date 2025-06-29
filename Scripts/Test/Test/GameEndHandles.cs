using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script connects an existing game-ending button with the ScoreSender
public class GameEndHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Button that ends the game")]
    public Button endGameButton;

    // Reference to the ScoreSender component
    private ScoreSender scoreSender;
    

    private void Start()
    {
        // Find the ScoreSender component
        scoreSender = FindObjectOfType<ScoreSender>();

        if (scoreSender == null)
        {
            Debug.LogError("No ScoreSender found in the scene! Please add the ScoreSender component to your ScoreManager GameObject.");
        }

        // Connect the button click event to send score and return to menu
        if (endGameButton != null && scoreSender != null)
        {
            endGameButton.onClick.AddListener(OnEndGameButtonClicked);
        }
        else if (endGameButton == null)
        {
            Debug.LogError("End Game Button reference is missing!");
        }
    }

    // private void OnEndGameButtonClicked()
    // {
    //     if (scoreSender != null)
    //     {
    //         scoreSender.SendScoreAndReturnToMenu();
    //     }
    // }

    public void OnEndGameButtonClicked()
    {
        if (scoreSender != null)
        {
            scoreSender.HandleGameEnd();
        }
    }

    // Alternative method for calling from Animation Events or other scripts
    public void EndGame()
    {
        if (scoreSender != null)
        {
            scoreSender.SendScoreAndReturnToMenu();
        }
    }
}