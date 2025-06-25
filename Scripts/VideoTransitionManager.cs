using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public VideoPlayer videoPlayer;
    public string nextSceneName;

    void Start()
    {
        if (videoPlayer != null)
        {
            fadeScreen.FadeIn();
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
        }
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

    void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(VideoEndRoutine());
    }

    IEnumerator VideoEndRoutine()
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        SceneManager.LoadScene("Game Menu");
    }
}
