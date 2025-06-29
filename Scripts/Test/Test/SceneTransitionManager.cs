using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    
    [Header("Name Input System")]
    public VRNameInputSystem nameInputSystem;

    private InputDevice targetDevice;
    private bool hasEnteredName = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);
            if (devices.Count > 0)
            {
                targetDevice = devices[0];
            }

            // Only clear name when starting the very first scene (press trigger scene)
            if (ScorePersistenceManager.Instance != null)
            {
                Debug.Log("SceneTransitionManager: Clearing player name for new session");
                ScorePersistenceManager.Instance.ClearCurrentPlayerName();
            }

            // Show name input system immediately
            if (nameInputSystem != null)
            {
                nameInputSystem.gameObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0 && targetDevice.isValid)
        {
            targetDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed);

            // Only allow trigger transition if name has been entered
            if (triggerPressed && HasValidName())
            {
                GoToScene(2);
            }
        }
    }

    private bool HasValidName()
    {
        if (ScorePersistenceManager.Instance != null)
        {
            string currentName = ScorePersistenceManager.Instance.GetCurrentPlayerName();
            return !string.IsNullOrEmpty(currentName);
        }
        return false;
    }

    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        SceneManager.LoadScene(sceneIndex);
    }

    // Call this method when returning to the main menu or when the application quits
    public void OnApplicationQuit()
    {
        if (ScorePersistenceManager.Instance != null)
        {
            ScorePersistenceManager.Instance.ClearCurrentPlayerName();
        }
    }

    // Call this when returning to main scene to reset name input
    public void ResetForNewSession()
    {
        if (ScorePersistenceManager.Instance != null)
        {
            ScorePersistenceManager.Instance.ClearCurrentPlayerName();
        }
        
        hasEnteredName = false;
        this.enabled = false;
        
        if (nameInputSystem != null)
        {
            nameInputSystem.gameObject.SetActive(true);
        }
    }
}