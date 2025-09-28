using UnityEngine;
using UnityEngine.Playables;

public class NoteSpawnPlayable : PlayableBehaviour
{
    public int lane;
    public string color;

    private bool spawned = false;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (spawned) return; // only spawn once
        spawned = true;

        // Dynamically find the spawner in the scene
        NoteSpawner spawner = GameObject.FindObjectOfType<NoteSpawner>();
        if (spawner != null)
        {
            spawner.SpawnNote(lane, color);
        }
        else
        {
            Debug.LogWarning("No NoteSpawner found in scene!");
        }
    }
}
