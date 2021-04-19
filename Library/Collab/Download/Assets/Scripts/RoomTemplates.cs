using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomTemplates : MonoBehaviour
{
    public Vector3[] roomOffsets;
    public GameObject[] floorTiles;
    public GameObject[] enemyTiles;
    public GameObject[] wallTiles;
    public GameObject[] outerWallTiles;
    public GameObject[] interactableTiles;
    public RoomTemplate[] templates;

    [Serializable]
    public class RoomTemplate
    {
        public Vector2 size;
        public List<Vector3> doorPositions;
        public int[] doorSides;
    }
}
