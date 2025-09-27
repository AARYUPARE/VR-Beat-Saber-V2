using UnityEngine;
using System; // Required for 'Action'

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;

    // BROADCAST STATIONS: Any other script can listen to these events.
    public static event Action<SongData> OnSongStarted;
    public static event Action OnSongStopped;

    [Header("Song List")]
    public SongData[] songs;

    private AudioSource audioSource;
    private SongData currentSong;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void LoadSong(int index)
    {
        if (index < 0 || index >= songs.Length) return;
        currentSong = songs[index];
        audioSource.clip = currentSong.audioClip;
    }



    public void PlaySong()
    {
        if (currentSong != null && audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log("Playing: " + currentSong.songName);

            // BROADCAST! Tell everyone that a song has started.
            OnSongStarted?.Invoke(currentSong);
        }
    }

    public void StopSong()
    {
        audioSource.Stop();

        // BROADCAST! Tell everyone that the song has stopped.
        OnSongStopped?.Invoke();
    }

    public SongData GetCurrentSong() => currentSong;
}