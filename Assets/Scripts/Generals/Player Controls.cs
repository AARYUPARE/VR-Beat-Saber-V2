using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] TimelinePlayer timelinePlayer;

    public void OnPlay(InputValue value)
    {
        if (!timelinePlayer) { return; }

        if (value.isPressed)
        {
            timelinePlayer.PlaySongTimeline(0); 
        }
    }
}
