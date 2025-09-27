using UnityEngine;

[System.Serializable]
public class SongData
{
    public string songName;
    public AudioClip audioClip;
    public TextAsset beatmapJSON;
    public float bpm;
}