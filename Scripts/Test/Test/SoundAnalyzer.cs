using UnityEngine;

public class SoundAnalyzer : MonoBehaviour
{
    public AudioSource audioSource;
    public int sampleSize = 1024;
    public FFTWindow fftWindow = FFTWindow.BlackmanHarris;

    private float[] samples;

    private void Start()
    {
        samples = new float[sampleSize];
    }

    public float GetAmplitude()
    {
        if (audioSource.isPlaying)
        {
            audioSource.GetSpectrumData(samples, 0, fftWindow);
            float amplitude = 0f;

            foreach (float sample in samples)
            {
                amplitude += sample;
            }

            return amplitude;
        }
        return 0f;
    }
}
