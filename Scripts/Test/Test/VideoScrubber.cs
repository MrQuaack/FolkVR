using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoScrubber : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Slider slider;
    private bool isDragging = false;

    void Start()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void Update()
    {
        if (videoPlayer.isPlaying && !isDragging)
        {
            slider.value = (float)(videoPlayer.time / videoPlayer.length);
        }
    }

    public void OnSliderChanged(float value)
    {
        if (isDragging)
        {
            double targetTime = value * videoPlayer.length;
            videoPlayer.time = targetTime;
        }
    }

    // Call these from the slider's Event Triggers
    public void StartDrag() => isDragging = true;
    public void EndDrag() => isDragging = false;
}
