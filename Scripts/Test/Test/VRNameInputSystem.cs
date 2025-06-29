using UnityEngine;
using TMPro;
using UnityEngine.XR;
using System.Collections.Generic;

public class VRNameInputSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nameInputPanel;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI nameDisplayText;
    public TextMeshProUGUI keyboardText;

    [Header("Virtual Keyboard")]
    public GameObject[] letterButtons; // Assign letter buttons A-Z
    public GameObject spaceButton;
    public GameObject backspaceButton;
    public GameObject confirmButton;

    private string currentName = "";
    private const int MAX_NAME_LENGTH = 12;
    
    private InputDevice targetDevice;
    private SceneTransitionManager sceneTransitionManager;

    // Virtual keyboard layout
    private string[] keyboardLayout = {
        "QWERTYUIOP",
        "ASDFGHJKL",
        "ZXCVBNM"
    };

    void Start()
    {
        // Get the scene transition manager
        sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
        
        // Get VR controller
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }

        // Initialize UI
        ShowNameInput();
    }

    private void ShowNameInput()
    {
        nameInputPanel.SetActive(true);
        instructionText.text = "Enter your name using the virtual keyboard";
        UpdateNameDisplay();
    }

    private void UpdateNameDisplay()
    {
        nameDisplayText.text = $"Name: {currentName}";
        
        // Update keyboard display (optional visual feedback)
        if (keyboardText != null)
        {
            string keyboardDisplay = "";
            foreach (string row in keyboardLayout)
            {
                keyboardDisplay += row + "\n";
            }
            keyboardText.text = keyboardDisplay;
        }
    }

    // Call this method when a letter button is pressed
    public void OnLetterPressed(string letter)
    {
        if (currentName.Length < MAX_NAME_LENGTH)
        {
            currentName += letter;
            UpdateNameDisplay();
        }
    }

    // Call this when space button is pressed
    public void OnSpacePressed()
    {
        if (currentName.Length < MAX_NAME_LENGTH && !currentName.EndsWith(" "))
        {
            currentName += " ";
            UpdateNameDisplay();
        }
    }

    // Call this when backspace button is pressed
    public void OnBackspacePressed()
    {
        if (currentName.Length > 0)
        {
            currentName = currentName.Substring(0, currentName.Length - 1);
            UpdateNameDisplay();
        }
    }

    // Call this when confirm button is pressed
    public void OnConfirmPressed()
    {
        if (!string.IsNullOrEmpty(currentName.Trim()))
        {
            string trimmedName = currentName.Trim();
            Debug.Log($"VRNameInputSystem: Attempting to save name: '{trimmedName}'");
            
            // Save the name to the persistence manager
            if (ScorePersistenceManager.Instance != null)
            {
                ScorePersistenceManager.Instance.SetCurrentPlayerName(trimmedName);
                
                // Verify it was saved correctly
                string savedName = ScorePersistenceManager.Instance.GetCurrentPlayerName();
                Debug.Log($"VRNameInputSystem: Verification - saved name is: '{savedName}'");
            }
            else
            {
                Debug.LogError("VRNameInputSystem: ScorePersistenceManager.Instance is null!");
            }

            // Hide the name input panel
            nameInputPanel.SetActive(false);

            // Update instruction to show trigger prompt
            instructionText.text = "Name saved! Press trigger to continue to game menu";
            instructionText.color = Color.green;

            Debug.Log($"VRNameInputSystem: Name confirmed: '{trimmedName}'. Player can now press trigger to continue.");
        }
        else
        {
            // Show error message
            instructionText.text = "Please enter a valid name!";
            instructionText.color = Color.red;
            Invoke("ResetInstructionText", 2f);
        }
    }

    private void ResetInstructionText()
    {
        instructionText.text = "Enter your name using the virtual keyboard";
        instructionText.color = Color.white;
    }

    // Alternative method for direct letter input (if you want to use keyboard input for testing)
    void Update()
    {
        // For testing purposes - remove this in VR-only build
        if (Input.inputString.Length > 0)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // Backspace
                {
                    OnBackspacePressed();
                }
                else if (c == '\n' || c == '\r') // Enter
                {
                    OnConfirmPressed();
                }
                else if (c == ' ') // Space
                {
                    OnSpacePressed();
                }
                else if (char.IsLetter(c) && currentName.Length < MAX_NAME_LENGTH)
                {
                    OnLetterPressed(c.ToString().ToUpper());
                }
            }
        }
    }
}