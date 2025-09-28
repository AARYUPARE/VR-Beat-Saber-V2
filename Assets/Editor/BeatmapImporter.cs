using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BeatmapImporter : EditorWindow
{
    public TextAsset beatmapJson;
    public PlayableDirector director;
    public TimelineAsset timeline;

    [MenuItem("Tools/Beatmap Importer")]
    public static void ShowWindow() => GetWindow<BeatmapImporter>("Beatmap Importer");

    void OnGUI()
    {
        beatmapJson = (TextAsset)EditorGUILayout.ObjectField("Beatmap JSON", beatmapJson, typeof(TextAsset), false);
        director = (PlayableDirector)EditorGUILayout.ObjectField("Playable Director", director, typeof(PlayableDirector), true);
        timeline = (TimelineAsset)EditorGUILayout.ObjectField("Timeline Asset", timeline, typeof(TimelineAsset), false);

        if (GUILayout.Button("Import Notes")) ImportNotes();
    }

    void ImportNotes()
    {
        if (beatmapJson == null || timeline == null)
        {
            Debug.LogError("Please assign Beatmap JSON and Timeline Asset!");
            return;
        }

        // Find or create NoteSpawnTrack
        NoteSpawnTrack noteTrack = null;
        foreach (var track in timeline.GetOutputTracks())
        {
            if (track is NoteSpawnTrack)
            {
                noteTrack = track as NoteSpawnTrack;
                break;
            }
        }
        if (noteTrack == null) noteTrack = timeline.CreateTrack<NoteSpawnTrack>(null, "Note Spawn Track");

        // Clear old clips
        foreach (var c in noteTrack.GetClips()) noteTrack.DeleteClip(c);

        // Parse JSON
        var beatmap = JsonUtility.FromJson<BeatmapWrapper>(beatmapJson.text);
        foreach (var note in beatmap.notes)
        {
            var clip = noteTrack.CreateClip<NoteSpawnPlayableAsset>();
            clip.start = note.time;
            clip.duration = 0.5f; // ensures visibility in Timeline

            var asset = clip.asset as NoteSpawnPlayableAsset;
            asset.lane = note.lane;
            asset.color = note.color;
        }

        // Assign timeline to director
        if (director != null && director.playableAsset != timeline) director.playableAsset = timeline;

        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();
        Debug.Log("✅ Beatmap imported and clips created!");
    }

    [System.Serializable]
    public class BeatmapWrapper { public Note[] notes; }

    [System.Serializable]
    public class Note { public float time; public int lane; public string color; }
}
