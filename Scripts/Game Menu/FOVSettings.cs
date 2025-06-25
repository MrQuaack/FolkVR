using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Import the TMP namespace

public class FOVSettings : MonoBehaviour
{
    public Camera vrCamera; // Assign your VR camera in the Inspector
    public float fovStep = 5f; // How much to increase/decrease FOV per button press
    public float minFOV = 60f; // Minimum FOV
    public float maxFOV = 120f; // Maximum FOV
    public TextMeshProUGUI fovText; // Reference to the TMP text element

    void Start()
    {
        // Ensure the FOV is initialized
        if (vrCamera != null)
        {
            vrCamera.fieldOfView = Mathf.Clamp(vrCamera.fieldOfView, minFOV, maxFOV);
            UpdateFOVText(); // Update the text at the start
        }
        else
        {
            Debug.LogError("VR Camera not assigned!");
        }
    }

    public void IncreaseFOV()
    {
        if (vrCamera != null)
        {
            vrCamera.fieldOfView = Mathf.Clamp(vrCamera.fieldOfView + fovStep, minFOV, maxFOV);
            UpdateFOVText(); // Update the text after increasing FOV
        }
    }

    public void DecreaseFOV()
    {
        if (vrCamera != null)
        {
            vrCamera.fieldOfView = Mathf.Clamp(vrCamera.fieldOfView - fovStep, minFOV, maxFOV);
            UpdateFOVText(); // Update the text after decreasing FOV
        }
    }

    private void UpdateFOVText()
    {
        if (fovText != null)
        {
            fovText.text = "FOV: " + vrCamera.fieldOfView.ToString("F1");
        }
    }
}
