using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance; // Singleton instance

    public AudioSource audioSource; // AudioSource to play music
    public AudioClip[] musicTracks; // Array of music tracks
    public bool loopPlaylist = true; // Whether to loop the entire playlist
    public float crossfadeDuration = 1.0f; // Duration of crossfade between tracks

    private int currentTrackIndex = -1; // Index of the currently playing track
    private bool isPlayingPlaylist = false; // Whether the playlist is currently playing

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    public void PlayPlaylist()
    {
        if (musicTracks.Length == 0)
        {
            Debug.LogWarning("No music tracks available!");
            return;
        }

        if (!isPlayingPlaylist)
        {
            isPlayingPlaylist = true;
            StartCoroutine(PlayPlaylistCoroutine());
        }
    }

    private IEnumerator PlayPlaylistCoroutine()
    {
        do
        {
            // Increment the track index
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;

            // Play the current track
            AudioClip currentTrack = musicTracks[currentTrackIndex];
            audioSource.clip = currentTrack;
            audioSource.loop = false; // Ensure individual tracks do not loop
            audioSource.Play();

            // Wait for the track to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            // Optional: Add a crossfade between tracks
            yield return StartCoroutine(CrossfadeOut());

        } while (loopPlaylist); // Repeat the playlist if looping is enabled

        isPlayingPlaylist = false; // Stop the playlist when done
    }

    private IEnumerator CrossfadeOut()
    {
        float elapsedTime = 0f;
        float initialVolume = audioSource.volume;

        while (elapsedTime < crossfadeDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(initialVolume, 0f, elapsedTime / crossfadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = initialVolume; // Restore volume for the next track
    }

    public void StopPlaylist()
    {
        StopAllCoroutines();
        audioSource.Stop();
        isPlayingPlaylist = false;
    }

    
}