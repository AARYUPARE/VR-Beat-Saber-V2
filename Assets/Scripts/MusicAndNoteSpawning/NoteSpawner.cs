using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject blueCube;
    public GameObject redCube;

    // This function gets called automatically by the setup
    public void SpawnNote(int lane, string color)
    {
        Vector3 spawnPos = new Vector3(lane * 2f, 1f, 10f); // adjust lane spacing
        if (color != "red" && color != "blue")
        {
            Debug.LogError("Invalid color specified for note: " + color);
            return;
        }
        else if (color == "red")
        {
            GameObject note = Instantiate(redCube, spawnPos, Quaternion.identity);
        }
        else
        {
            GameObject note = Instantiate(blueCube, spawnPos, Quaternion.identity);
        }

        // // Optional: set color
        // Renderer rend = note.GetComponent<Renderer>();
        // if (rend != null)
        // {
        //     if (color == "red") rend.material.color = Color.red;
        //     else if (color == "blue") rend.material.color = Color.blue;
        // }
    }
}
