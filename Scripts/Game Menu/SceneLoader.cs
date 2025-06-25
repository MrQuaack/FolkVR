using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene()
    {
        Debug.Log("Button Pressed: Loading Sinulog Scene...");
        SceneManager.LoadScene("Sinulog");
    }
}
