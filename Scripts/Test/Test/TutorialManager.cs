using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Added for scene management

public class TutorialManager : MonoBehaviour
{
    [Header("NPC References")]
    public Animator npcAnimator;
    public AudioSource voiceOverAudio;

    [Header("UI References")]
    public Button nextStepButton;
    public Button restartStepButton;
    public Button previousStepButton;
    public TextMeshProUGUI tutorialText;
    
    [Header("Fader References")]
    public FadeScreen fadeScreen; // Reference to your FadeScreen script

    [Header("Tutorial Steps")]
    public string[] stepDescriptions;
    public AudioClip[] voiceOverClips;
    public string idleStateName = "Sinulog Idle";
    public string[] danceStateNames = {
        "DanceStep1", "DanceStep2", "DanceStep3",
        "DanceStep4", "DanceStep5", "DanceStep6",
        "DanceStep7", "DanceStep8", "DanceStep9"
    };

    [Header("End Tutorial Settings")]
    public AudioClip outroAudioClip; // Reference for the outro audio
    public string menuSceneName = "GameMenu"; // Name of the menu scene to load

    private int currentStep = 0;
    private bool isAnimating = false;
    private bool isTutorialEnding = false; // Flag to track if the tutorial is ending

    private void Start()
    {
        // Initialize buttons
        nextStepButton.onClick.AddListener(NextStep);
        restartStepButton.onClick.AddListener(RestartStep);
        previousStepButton.onClick.AddListener(PreviousStep);

        // Set default text for the first step
        if (stepDescriptions != null && stepDescriptions.Length > 0)
        {
            tutorialText.text = stepDescriptions[0];
        }

        StartStep(currentStep);
    }

    private void StartStep(int stepIndex)
    {
        // Check for valid indices
        if (stepDescriptions == null || stepIndex < 0 || stepIndex >= stepDescriptions.Length)
        {
            Debug.LogError($"Invalid step index: {stepIndex}. Available steps: {(stepDescriptions != null ? stepDescriptions.Length : 0)}");
            return;
        }

        if (danceStateNames == null || stepIndex >= danceStateNames.Length)
        {
            Debug.LogError($"Invalid dance state index: {stepIndex}. Available states: {(danceStateNames != null ? danceStateNames.Length : 0)}");
            return;
        }

        // Update tutorial text
        tutorialText.text = stepDescriptions[stepIndex];

        // Play the voice-over
        if (voiceOverAudio != null && voiceOverClips != null && stepIndex < voiceOverClips.Length && voiceOverClips[stepIndex] != null)
        {
            voiceOverAudio.clip = voiceOverClips[stepIndex];
            voiceOverAudio.Play();
        }

        // Trigger the NPC animation by direct state transition
        if (npcAnimator != null && !isAnimating)
        {
            isAnimating = true;

            // Get the current state info
            string stateName = danceStateNames[stepIndex];
            Debug.Log($"Playing dance state: {stateName}");

            // Play the animation using CrossFade for smoother transition
            npcAnimator.CrossFade(stateName, 0.25f, 0);

            // Start coroutine to wait and return to idle
            StartCoroutine(MonitorAnimationAndReturnToIdle(stateName));
        }
    }

    private IEnumerator MonitorAnimationAndReturnToIdle(string stateName)
    {
        int layerIndex = 0;

        // Wait a short time to ensure the animation starts
        yield return new WaitForSeconds(0.3f);

        // Wait until the animation is finished or nearly finished
        bool animationPlaying = true;
        float checkInterval = 0.1f;
        float totalWaitTime = 0f;
        float maxWaitTime = 10f; // Safety timeout

        while (animationPlaying && totalWaitTime < maxWaitTime)
        {
            AnimatorStateInfo stateInfo = npcAnimator.GetCurrentAnimatorStateInfo(layerIndex);

            // Check if we're in the target state and if it's nearly complete
            if (stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 0.9f)
            {
                animationPlaying = false;
            }
            else if (!stateInfo.IsName(stateName) && totalWaitTime > 1.0f)
            {
                // If we're no longer in the expected state after 1 second,
                // the animation might have already finished or transitioned
                animationPlaying = false;
            }

            yield return new WaitForSeconds(checkInterval);
            totalWaitTime += checkInterval;
        }

        // Return to idle state
        npcAnimator.CrossFade(idleStateName, 0.25f, 0);

        // Add a short delay before allowing next action
        yield return new WaitForSeconds(0.5f);
        isAnimating = false;
    }

    private void NextStep()
    {
        // Only proceed if not currently animating and not already ending
        if (!isAnimating && !isTutorialEnding)
        {
            // Check if this is the last step
            bool isLastStep = (currentStep == danceStateNames.Length - 1);
            
            if (isLastStep)
            {
                // Start the ending sequence
                StartCoroutine(EndTutorialSequence());
            }
            else
            {
                // Move to the next step normally
                currentStep++;
                StartStep(currentStep);
            }
        }
    }

    private IEnumerator EndTutorialSequence()
    {
        isTutorialEnding = true;
        
        // Disable buttons during the ending sequence
        nextStepButton.interactable = false;
        restartStepButton.interactable = false;
        previousStepButton.interactable = false;
        
        // Play outro audio if available
        if (outroAudioClip != null && voiceOverAudio != null)
        {
            voiceOverAudio.clip = outroAudioClip;
            voiceOverAudio.Play();
            
            // Wait for the outro audio to finish
            float audioDuration = outroAudioClip.length;
            yield return new WaitForSeconds(audioDuration);
        }
        
        // Trigger the fade out using your existing FadeScreen component
        if (fadeScreen != null)
        {
            // Call the FadeOut method on your FadeScreen script
            fadeScreen.FadeOut();
            
            // Wait for the fade to complete using the duration from your FadeScreen
            yield return new WaitForSeconds(fadeScreen.fadeDuration + 0.5f);
        }
        else
        {
            Debug.LogWarning("No FadeScreen component assigned - fade effect will be skipped.");
            // If no fader, just wait a moment
            yield return new WaitForSeconds(1.0f);
        }
        
        // Load the game menu scene
        SceneManager.LoadScene(menuSceneName);
    }

    private void RestartStep()
    {
        // Only restart if not currently animating and not ending
        if (!isAnimating && !isTutorialEnding)
        {
            StartStep(currentStep);
        }
    }

    private void PreviousStep()
    {
        // Only go to the previous step if not currently animating and not ending
        if (!isAnimating && !isTutorialEnding)
        {
            // Move to the previous step
            currentStep--;

            // Loop back to the last step if the index goes below 0
            if (stepDescriptions != null && currentStep < 0)
            {
                currentStep = stepDescriptions.Length - 1;
            }

            // Start the previous step
            StartStep(currentStep);
        }
    }
}