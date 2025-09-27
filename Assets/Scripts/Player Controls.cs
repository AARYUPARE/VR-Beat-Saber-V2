using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    SongManager sm;

    private void Awake()
    {
        sm = FindAnyObjectByType<SongManager>();
    }

    public void OnPlay(InputValue value)
    {
        if (value.isPressed)
        {
            sm.LoadSong(0);
            sm.PlaySong();
        }
    }
}
