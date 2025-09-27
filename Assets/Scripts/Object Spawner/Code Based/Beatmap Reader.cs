using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class BeatmapReader : MonoBehaviour
{
    public GameObject redBlockPrefab;
    public GameObject blueBlockPrefab;

    [SerializeField] float height = 1.0f;
    [SerializeField] float forwardDistance = 10f;

    // Subscribe to the broadcasts when this object is enabled
    private void OnEnable()
    {
        SongManager.OnSongStarted += HandleSongStarted;
        SongManager.OnSongStopped += HandleSongStopped;
    }

    // Unsubscribe when disabled to prevent memory leaks
    private void OnDisable()
    {
        SongManager.OnSongStarted -= HandleSongStarted;
        SongManager.OnSongStopped -= HandleSongStopped;
    }

    // This function gets called automatically when the OnSongStarted broadcast is sent.
    private void HandleSongStarted(SongData song)
    {
        Debug.Log("BeatmapReader heard that " + song.songName + " has started!");
        StartCoroutine(ProcessBeatmap(song));
    }

    // This function is called when the OnSongStopped broadcast is sent.
    private void HandleSongStopped()
    {
        Debug.Log("BeatmapReader heard that the song has stopped. Stopping all spawns.");
        StopAllCoroutines();
    }

    private IEnumerator ProcessBeatmap(SongData song)
    {
        if (song.beatmapJSON == null)
        {
            Debug.LogError("Beatmap JSON is missing!");
            yield break; // Exit the coroutine
        }

        Beatmap beatmap = JsonUtility.FromJson<Beatmap>(song.beatmapJSON.text);
        float lastNoteTime = 0f;

        foreach (var note in beatmap.notes)
        {
            // Calculate how long to wait from the last note's time until this new note's time
            float timeToWait = note.time - lastNoteTime;
            yield return new WaitForSeconds(timeToWait);

            // Once we've waited, it's time to spawn this note
            SpawnNote(note);

            // Update the time of the last note we spawned
            lastNoteTime = note.time;
        }

        Debug.Log("Finished processing beatmap.");
    }

    void SpawnNote(BeatmapNote note)
    {
        GameObject prefab = note.color == "red" ? redBlockPrefab : blueBlockPrefab;
        Vector3 spawnPos = new Vector3(note.lane, height, forwardDistance); // Example lane system
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}