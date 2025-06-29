using UnityEngine;
using UnityEngine.Video;

public class VideoInteraction : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void OnMouseDown() // Works in Editor with mouse clicks
    {
        ToggleVideo();
    }

    public void ToggleVideo()
    {
        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }
}
