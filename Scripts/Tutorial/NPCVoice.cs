using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCVoice : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip introClip;
    public AudioClip raiseHandClip;
    public AudioClip greatJobClip;
    public AudioClip tryAgainClip;
    public AudioClip secondStepClip;
    public AudioClip thirdStepClip;
    public AudioClip fourthStepClip;
    public AudioClip announceClip;

    public Animator npcAnimator;

    public FadeScreen fadeScreen; // Reference to the FadeScreen script

    private bool isPlayingIntro = false;
    private int currentStep = 1; // Step counter

    void Start()
    {
        PlayIntro();
    }

    public void PlayIntro()
    {
        isPlayingIntro = true;
        audioSource.PlayOneShot(introClip);
        npcAnimator.SetBool("Breathing", true);
        StartCoroutine(WaitForClipToFinish(introClip, () =>
        {
            isPlayingIntro = false;
            npcAnimator.SetBool("Breathing", false);
            PlayRaiseHand();
        }));
    }

    public void PlayRaiseHand()
    {
        if (!isPlayingIntro)
        {
            audioSource.PlayOneShot(raiseHandClip);
            npcAnimator.SetBool("HandRaise", true);
            Debug.Log("Raise your hand voice line played.");
        }
    }

    public void PlayGreatJob()
    {
        if (!isPlayingIntro)
        {
            audioSource.PlayOneShot(greatJobClip);
            npcAnimator.SetBool("Breathing", true);
            Debug.Log("Great job voice line played.");

            // Call the next step after the current great job finishes
            StartCoroutine(WaitForClipToFinish(greatJobClip, AdvanceToNextStep));
        }
    }

    private void AdvanceToNextStep()
    {
        currentStep++;
        Debug.Log($"Advancing to step {currentStep}");

        switch (currentStep)
        {
            case 2:
                PlaySecondStep();
                break;
            case 3:
                PlayThirdStep();
                break;
            case 4:
                PlayFourthStep();
                break;
            case 5:
                PlayAnnounce();
                break;
            default:
                Debug.LogWarning("No more steps!");
                break;
        }
    }

    public void PlayTryAgain()
    {
        if (!isPlayingIntro)
        {
            audioSource.PlayOneShot(tryAgainClip);
            Debug.Log("Try again voice line played.");
        }
    }

    public void PlaySecondStep()
    {
        audioSource.PlayOneShot(secondStepClip);
        npcAnimator.SetBool("Breathing", true);
        Debug.Log("Let's begin the second step voice line played.");
    }

    public void PlayThirdStep()
    {
        audioSource.PlayOneShot(thirdStepClip);
        npcAnimator.SetBool("CrossArm", true);
        Debug.Log("Third step voice line played.");
    }

    public void PlayFourthStep()
    {
        audioSource.PlayOneShot(fourthStepClip);
        npcAnimator.SetBool("HandRaise", true);
        Debug.Log("Fourth step voice line played.");
    }

    public void PlayAnnounce()
    {
        audioSource.PlayOneShot(announceClip);
        Debug.Log("Announce voice line played. Goodbye!");

        StartCoroutine(WaitForClipToFinish(announceClip, () =>
        {
            fadeScreen.FadeOut(); // Trigger fade-out
            StartCoroutine(WaitForFadeToComplete(() => SceneManager.LoadScene("Game Menu")));
        }));
    }

    private IEnumerator WaitForFadeToComplete(System.Action callback)
    {
        yield return new WaitForSeconds(fadeScreen.fadeDuration); // Wait for the fade duration
        callback?.Invoke();
    }

    private IEnumerator WaitForClipToFinish(AudioClip clip, System.Action callback)
    {
        yield return new WaitForSeconds(clip.length);
        callback?.Invoke();
    }
}
