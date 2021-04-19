using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonManager2 : MonoBehaviour
{
    public RoomTemplates roomTemplates;
    public int roomCount;
    public List<Vector2> occupiedPositions = new List<Vector2>();
    public List<Room> levelMap = new List<Room>();

    private GameObject toInstantiate;
    private int difficultyLevel;
    public void SetupScene(int level)
    {
        difficultyLevel = level;

        roomTemplates = gameObject.GetComponent<RoomTemplates>();

        Transform firstRoomTransform = new GameObject("Room " + levelMap.Count).transform;
        Room firstRoom = firstRoomTransform.gameObject.AddComponent<Room>();
        firstRoom.position = new Vector3(0f, 0f, 0f);
        firstRoom.doors = new int[4] { 1, 1, 1, 1 };
        firstRoom.size = roomTemplates.templates[0].size;
        occupiedPositions.Add(firstRoom.position);
        CreateRoom(firstRoom);
        levelMap.Add(firstRoom);

        for (int i = 0; i < firstRoom.doors.Length; i++)
        {
            if (i % 2 == 0)
                NewCorridor(Vector3.zero, roomTemplates.roomOffsets[i], 2, i);
            else
                NewCorridor(Vector3.zero, roomTemplates.roomOffsets[i], 1, i);
        }
    }

    private void NewCorridor(Vector3 lastPosition, Vector3 offset, int corridorType, int side)
    {
        Vector3 position = new Vector3();
        position = lastPosition + offset;
        occupiedPositions.Add(position);

        StartCoroutine(NewRoom(position + offset, side, 0.01f));

        //Смещение коридора к центру
        if (corridorType == 1)
        {
            position += new Vector3(0f, 3.5f, 0f);
            CreateHorrizontalCorridor(position);
        }
        else
        {
            position += new Vector3(4f, 0f, 0f);
            CreateVerticalCorridor(position);
        }
    }

    private IEnumerator NewRoom(Vector3 position, int side, float waitTime)
    {
        Transform newRoomTransform = new GameObject("Room" + levelMap.Count).transform;
        Room newRoom = newRoomTransform.gameObject.AddComponent<Room>();
        newRoom.doors = new int[4] { 0, 0, 0, 0 };
        newRoom.size = roomTemplates.templates[0].size;
        newRoom.position = position;

        //Установка дверей из входящих коридоров
        switch (side)
        {
            case 0:
                newRoom.doors[2]++;
                break;
            case 1:
                newRoom.doors[3]++;
                break;
            case 2:
                newRoom.doors[0]++;
                break;
            case 3:
                newRoom.doors[1]++;
                break;
        }

        levelMap.Add(newRoom);
        occupiedPositions.Add(position);
        CreateRoom(newRoom);

        //Думаем уже о соседней комнате
        for (int i = 0; i < newRoom.doors.Length; i++)
        {
           yield return new WaitForSeconds(waitTime);

           if (levelMap.Count <= roomCount)
            {
                if (!occupiedPositions.Contains(position + roomTemplates.roomOffsets[i]) && !occupiedPositions.Contains(position + roomTemplates.roomOffsets[i] * 2))
                {
                    /*int makeDoorRoll = Random.Range(1, 100);
                    bool makeDoor = makeDoorRoll > 50;
                    newRoom.doors[i] = makeDoor ? 1 : 0;
                    if (makeDoor)*/
                        NewCorridor(position, roomTemplates.roomOffsets[i], (i % 2 == 0 ? 2 : 1), i);
                }
            }
        }
    }

    private void CreateRoom(Room room)
    {
        for (int x = 0; x < room.size.x; x++)
        {
            for (int y = 0; y < room.size.y; y++)
            {
                toInstantiate = roomTemplates.floorTiles[Random.Range(0, roomTemplates.floorTiles.Length)];

                if (y == room.size.y - 1)
                    toInstantiate = roomTemplates.outerWallTiles[1];
                if (x == room.size.x - 1)
                    toInstantiate = roomTemplates.outerWallTiles[3];
                if (y == 0)
                    toInstantiate = roomTemplates.outerWallTiles[5];
                if (x == 0)
                    toInstantiate = roomTemplates.outerWallTiles[7];
                if (x == 0 && y == room.size.y - 1)
                    toInstantiate = roomTemplates.outerWallTiles[0];
                if (x == room.size.x - 1 && y == room.size.y - 1)
                    toInstantiate = roomTemplates.outerWallTiles[2];
                if (x == room.size.x - 1 && y == 0)
                    toInstantiate = roomTemplates.outerWallTiles[4];
                if (x == 0 && y == 0)
                    toInstantiate = roomTemplates.outerWallTiles[6];
                if (roomTemplates.templates[0].doorPositions.Contains(new Vector2(x, y)))
                {
                    room.doorPositions.Add(room.position + new Vector3(x, y, 0f));
                    continue;
                }

                GameObject instance = Instantiate(toInstantiate,room.position + new Vector3(x, y, 0f), Quaternion.identity);
                instance.transform.SetParent(room.gameObject.transform);
            }
        }
    }

    private void CreateHorrizontalCorridor(Vector3 position)
    {
        Transform corridorHolder = new GameObject("HorizontalCorridor").transform;
        for (int x = 0; x < roomTemplates.templates[0].size.x; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                toInstantiate = roomTemplates.floorTiles[Random.Range(0, roomTemplates.floorTiles.Length)];
                if (y == 0)
                    toInstantiate = roomTemplates.outerWallTiles[5];
                if (y == 4)
                    toInstantiate = roomTemplates.outerWallTiles[1];

                GameObject instance = Instantiate(toInstantiate, position + new Vector3(x, y, 0f), Quaternion.identity);
                instance.transform.SetParent(corridorHolder);
            }
        }
    }

    private void CreateVerticalCorridor(Vector3 position)
    {
        Transform corridorHolder = new GameObject("VerticalCorridor").transform;
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < roomTemplates.templates[0].size.y; y++)
            {
                toInstantiate = roomTemplates.floorTiles[Random.Range(0, roomTemplates.floorTiles.Length)];
                if (x == 0)
                    toInstantiate = roomTemplates.outerWallTiles[7];
                if (x == 4)
                    toInstantiate = roomTemplates.outerWallTiles[3];

                GameObject instance = Instantiate(toInstantiate, position + new Vector3(x, y, 0f), Quaternion.identity);
                instance.transform.SetParent(corridorHolder);
            }
        }
    }

    private void PopulateRoom(Vector3 position, Vector2 size)
    {

    }

    private void InitialiseList(List<Vector3> roomFreePositions, Vector2 size)
    {
        roomFreePositions.Clear();

        for (int x = 1; x < size.x - 1; x++)
        {
            for (int y = 1; y < size.y - 1; y++)
            {
                roomFreePositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    private Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, occupiedPositions.Count);
        Vector3 randomPosition = occupiedPositions[randomIndex];
        occupiedPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    private void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        int objectCount = Random.Range(minimum, maximum + 1);
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }
}