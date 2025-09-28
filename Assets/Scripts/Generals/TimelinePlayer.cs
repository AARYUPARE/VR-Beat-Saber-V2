using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelinePlayer : MonoBehaviour
{
    [SerializeField] List<TimelineAsset> songTimelines;

    PlayableDirector playableDirector;
    private bool playing = false;

    private void Awake()
    {
        playableDirector = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        if(!playableDirector) { return; }
        playableDirector.played += SetPlayStatusOn;        
        playableDirector.paused += SetPlayStatusOff;        
    }
    
    private void OnDisable()
    {
        if(!playableDirector) { return; }
        playableDirector.played -= SetPlayStatusOn;        
        playableDirector.paused -= SetPlayStatusOff;        
    }

    public void SetPlayStatusOn(PlayableDirector pd)
    {
        playing = true;
    }
   
    public void SetPlayStatusOff(PlayableDirector pd)
    {
        playing = false;
    }

    public void PlaySongTimeline(int ind)
    {
        if(ind < 0 || ind >= songTimelines.Count) { return; }
        if (playing || !playableDirector) { return; }

        var timeline = songTimelines[ind];        
        playableDirector.Play(timeline);
    }

    public void StopAll()
    {
        if (!playableDirector) { return; }

        playableDirector.Stop();
    }
}
