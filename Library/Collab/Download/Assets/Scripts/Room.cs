using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Vector3 position;
    public Vector2 size;
    public int[] doors;
    public List<Vector3> freePositions;
    public List<Vector3> doorPositions;

    public Room()
    {
        doorPositions = new List<Vector3>();
        freePositions = new List<Vector3>();
    }
}
