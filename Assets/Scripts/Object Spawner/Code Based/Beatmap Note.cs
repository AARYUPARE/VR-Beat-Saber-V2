using UnityEngine;

// Note: Beatmap and BeatmapNote classes are still here, no changes needed for them.
[System.Serializable]
public class BeatmapNote
{
    public float time;    // Time in seconds
    public int lane;      // Which lane (left, right, up, etc.)
    public string color;  // Red/Blue block
}