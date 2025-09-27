using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] List<GameObject> spawningObjects;

    [SerializeField] float[] lanes = { -0.5f, 0.5f };

    [SerializeField] float height = 0.5f;
    [SerializeField] float forwardDistance = 25.0f;

    int selectedObject = 0;

    bool setRandom = false;

    public void ChangeObject()
    {
        int newSelectedObject = selectedObject;

        while(newSelectedObject == selectedObject)
        {
            newSelectedObject = Random.Range(0, spawningObjects.Count);
        }

        selectedObject = newSelectedObject;
    }

    public void SpawnObject()
    {
        if(setRandom) { ChangeObject(); }

        Vector3 spawnPosition = GetSpawnPosition();

        Instantiate(spawningObjects[selectedObject], spawnPosition, Quaternion.identity);
    }

    private Vector3 GetSpawnPosition()
    {
        int lane = Random.Range(0, lanes.Length);

        return new Vector3(lanes[lane], height, forwardDistance);
    }

    public void SetRandomOn()
    {
        setRandom = true;
    }

    public void SetRandomOff()
    {
        setRandom = false;
    }
}
