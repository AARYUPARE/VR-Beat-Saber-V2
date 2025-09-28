using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class NoteSpawnPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public int lane;
    public string color;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<NoteSpawnPlayable>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.lane = lane;
        behaviour.color = color;
        return playable;
    }
}
