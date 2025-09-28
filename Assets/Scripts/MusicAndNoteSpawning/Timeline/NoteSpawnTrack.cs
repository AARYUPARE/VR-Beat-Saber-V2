using UnityEngine.Timeline;

[TrackColor(0.8f, 0.1f, 0.1f)]
[TrackClipType(typeof(NoteSpawnPlayableAsset))] // matches the playable asset
public class NoteSpawnTrack : TrackAsset { }
