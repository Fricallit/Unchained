using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathingNode
{
    public bool walkable = true;
    public GameObject obj = null;
}

public class DungeonManager : MonoBehaviour
{
    //Высота и Ширина поля
    public int boardRows, boardColumns;
    //Mаксимальный и минимальный размер одной стороны для деления поля на участки
    public int minLeafSize, maxLeafSize;
    //Максимальное количество разбиений поля
    public int maxSplits;
    //Рисовать ли границы участков для дебага?
    public bool shouldDebugDrawBSP;
    //Количество 
    public Count interactablesCount;

    //Вершина дерева разбиений
    private SubDungeon firstNode = null;
    //Координаты свободных клетки, в которые можно поставить предметы и врагов
    private List<Vector2Int> freePositions;
    //Данные о проходимости карты для навигации, ссылки на объекты в клетках
    private PathingNode[,] pathingPositions;
    //Все листья дерева разбиений (участки, содержащие 1 комнату)
    private List<SubDungeon> leaves;
    //Количество текущих разбиений поля
    private static int splits = 0;
    //Объект для группировки обстановки уровня в иерархии объектов
    private Transform dungeonHolder;
    //Объект для группировки предметов и врагов в иерархии объектов
    private Transform entityHolder;
    //Количество внутренних стен и препятствий, рассчитывается от размера уровня
    private Count blocksCount;
    //Объект, содержащий ссылки на префабы для клонирования
    private ObjectReferences []tiles;
    //Тайлсет на текущем уровне
    private int activeTileset;

    //Класс для удобного представления в редакторе диапазонов
    [Serializable]
    public class Count
    {
        public int maximum;
        public int minimum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    //Класс для узла дерева разделения
    public class SubDungeon
    {
        public SubDungeon left, right;
        public List<Rect> corridors = new List<Rect>();
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0);
        //Идентификатор узла дерева для дебага
        public int debugId;
        public RoomType roomType;

        //Тип комнаты влияет на допустимость в ней объектов, notSet - не лист дерева
        public enum RoomType
        {
            notSet, regularRoom, startRoom, endRoom
        }

        private static int debugCounter = 0;

        public SubDungeon(Rect mrect)
        {
            rect = mrect;
            debugId = debugCounter;
            debugCounter++;
            roomType = RoomType.notSet;
        }

        //Является ли узел листом?
        public bool IAmLeaf()
        {
            return left == null && right == null;
        }
        //Разделить участок узла на два
        public bool Split(int minLeafSize, int maxLeafSize)
        {
            if (!IAmLeaf())
            {
                return false;
            }

            bool splitH;
            if (rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }
            else if (rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else
            {
                splitH = Random.Range(0.0f, 1.0f) > 0.5;
            }

            if (Mathf.Max(rect.height, rect.width) / 2 < minLeafSize)
            {
                //Debug.Log("Sub-dungeon " + debugId + " will be a leaf");
                return false;
            }

            if (splitH)
            {
                int split = Random.Range(minLeafSize, (int)(rect.height - minLeafSize));

                if (Mathf.Abs(rect.height - split) < minLeafSize)
                    split = (int)(rect.height / 2);

                left = new SubDungeon(new Rect(rect.x, rect.y, rect.width, split));
                right = new SubDungeon(new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            else
            {
                int split = Random.Range(minLeafSize, (int)(rect.width - minLeafSize));

                if (Mathf.Abs(rect.width - split) < minLeafSize)
                    split = (int)(rect.width / 2);

                left = new SubDungeon(new Rect(rect.x, rect.y, split, rect.height));
                right = new SubDungeon(new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }
            splits++;
            return true;
        }
        //Определить размеры комнат
        public void CreateRoom()
        {
            if (left != null)
            {
                left.CreateRoom();
            }
            if (right != null)
            {
                right.CreateRoom();
            }
            if (left != null && right != null)
            {
                CreateCorridorBetween(left, right);
            }
            if (IAmLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 1.7f, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 1.7f, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                room = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);
                roomType = RoomType.regularRoom;
                //Debug.Log("Created room " + room + " in sub-dungeon " + debugId + ": " + rect);
            }
        }
        //Получить прямоугльник комнаты
        public Rect GetRoom()
        {
            if (IAmLeaf())
            {
                return room;
            }
            if (left != null)
            {
                Rect lroom = left.GetRoom();
                if (lroom.x != -1)
                {
                    return lroom;
                }
            }
            if (right != null)
            {
                Rect rroom = right.GetRoom();
                if (rroom.x != -1)
                {
                    return rroom;
                }
            }
            return new Rect(-1, -1, 0, 0);
        }
        //Определить размер и положение коридора между комнатами двух узлов
        public void CreateCorridorBetween(SubDungeon left, SubDungeon right)
        {
            Rect lroom = left.GetRoom();
            Rect rroom = right.GetRoom();

            //Debug.Log("Creating corridor(s) between " + left.debugId + "(" + lroom + ") and " + right.debugId + " (" + rroom + ")");

            Vector2 lpoint = new Vector2((int)Random.Range(lroom.x + 2, lroom.xMax - 2), (int)Random.Range(lroom.y + 2, lroom.yMax - 2));
            Vector2 rpoint = new Vector2((int)Random.Range(rroom.x + 2, rroom.xMax - 2), (int)Random.Range(rroom.y + 2, rroom.yMax - 2));

            if (lpoint.x > rpoint.x)
            {
                Vector2 temp = lpoint;
                lpoint = rpoint;
                rpoint = temp;
            }

            int w = (int)(lpoint.x - rpoint.x);
            int h = (int)(lpoint.y - rpoint.y);

            //Debug.Log("lpoint: " + lpoint + ", rpoint: " + rpoint + ", w: " + w + ", h: " + h);

            if (w != 0)
            {
                if (Random.Range(0, 1) > 2)
                {
                    corridors.Add(new Rect(lpoint.x, lpoint.y, Mathf.Abs(w) + 1, 1));
                    if (h < 0)
                    {
                        corridors.Add(new Rect(rpoint.x, lpoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(rpoint.x, lpoint.y, 1, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    if (h < 0)
                    {
                        corridors.Add(new Rect(lpoint.x, lpoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(lpoint.x, rpoint.y, 1, Mathf.Abs(h)));
                    }
                    corridors.Add(new Rect(lpoint.x, rpoint.y, Mathf.Abs(w) + 1, 1));
                }
            }
            else
            {
                if (h < 0)
                {
                    corridors.Add(new Rect((int)lpoint.x, (int)lpoint.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridors.Add(new Rect((int)rpoint.x, (int)rpoint.y, 1, Mathf.Abs(h)));
                }
            }

            /*Debug.Log("Corridors: ");
            foreach (Rect corridor in corridors)
            {
                Debug.Log("corridor: " + corridor);
            }*/
        }
    }
    //Создать дерево разделения
    private void CreateBSP(SubDungeon subdungeon)
    {
        //Debug.Log("Splitting sub-dungeon " + subdungeon.debugId + ": " + subdungeon.rect);
        if (subdungeon.IAmLeaf())
        {
            if (splits < maxSplits)
            {
                if (subdungeon.rect.width > maxLeafSize || subdungeon.rect.height > maxLeafSize)
                {
                    if (subdungeon.Split(minLeafSize, maxLeafSize))
                    {
                        /*Debug.Log("Splitted sub-dungeon " + subdungeon.debugId + " in "
                            + subdungeon.left.debugId + ": " + subdungeon.left.rect + ", "
                            + subdungeon.right.debugId + ": " + subdungeon.right.rect);*/

                        CreateBSP(subdungeon.left);
                        CreateBSP(subdungeon.right);
                        return;
                    }
                    else
                    {
                        leaves.Add(subdungeon);
                        return;
                    }
                }
            }
            leaves.Add(subdungeon);
        }
    }
    //Создает игровой объект в определенных координатах, добавляет его в pathingPositions
    private GameObject CreateAt(int x, int y, GameObject toInstantiate)
    {
        GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity);
        instance.transform.SetParent(dungeonHolder.transform);
        pathingPositions[x, y] = new PathingNode();
        if (toInstantiate.CompareTag("Wall"))
        {
            pathingPositions[x, y].walkable = false;
        }
        pathingPositions[x, y].obj = instance;
        return instance;
    }
    //Создает игровой объект в координатах, даже если там уже что-то есть. Нужен для соединения стен коридоров с комнатами при отрисовке.
    private GameObject PlaceAbove(int x, int y, GameObject toInstantiate)
    {
        GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity);
        instance.transform.SetParent(dungeonHolder.transform);
        return instance;
    }
    //Создает игровой объект в координатах, если там нет других объектов
    private GameObject CreateIfNotOccupied(int x, int y, GameObject toInstantiate)
    {
        if (pathingPositions[x, y] == null)
        {
            GameObject instance =  CreateAt(x, y, toInstantiate);
            return instance;
        }
        else
        {
            return null;
        }
    }
    //Удаляет старый объект, создает новый на его координатах
    private GameObject Replace(int x, int y, GameObject toInstantiate)
    {
        if (pathingPositions[x, y] != null)
        {
            Destroy(pathingPositions[x, y].obj);
            pathingPositions[x, y].obj = null;
            pathingPositions[x, y] = null;
        }
        GameObject instance = CreateAt(x, y, toInstantiate);
        return instance;
    }
    //Выбирает из массива объектов с определенной вероятностью объект (вариант) для клонирования. Используется для отрисовки полов, создания предметов.
    private GameObject RandomTile(GameObject[] tileArray, float variationProbability)
    {
        int randomIndex;
        if (Random.Range(0.0f, 1.0f) < variationProbability)
            randomIndex = Random.Range(0, tileArray.Length - 1);
        else
            randomIndex = 0;

        return tileArray[randomIndex];
    }
    //Поворачивает объект на случайный (не совсем) угол. Нужно для пола
    private void RandomRotation(GameObject tile)
    {
        tile.transform.Rotate(0, 0, (180 * (int)Random.Range(0, 2)));
    }
    //Отрисовка комнат
    private void DrawRooms(SubDungeon subdungeon)
    {
        if (subdungeon == null)
        {
            return;
        }

        if (subdungeon.IAmLeaf())
        {
            GameObject toInstantiate;
            for (int i = (int)subdungeon.room.x; i < subdungeon.room.xMax; i++)
            {
                for (int j = (int)subdungeon.room.y; j < subdungeon.room.yMax; j++)
                {
                    toInstantiate = RandomTile(tiles[activeTileset].floorTiles, 0.15f);

                    if (i == subdungeon.room.x || i == subdungeon.room.xMax - 1
                        || j == subdungeon.room.y || j == subdungeon.room.yMax - 1)
                    {
                        if (i == subdungeon.room.xMax - 1)
                            toInstantiate = tiles[activeTileset].wallTiles[2];
                        else if (i == subdungeon.room.x)
                            toInstantiate = tiles[activeTileset].wallTiles[6];
                        else if (j == subdungeon.room.y)
                            toInstantiate = tiles[activeTileset].wallTiles[4];
                        else if (j == subdungeon.room.yMax - 1)
                            toInstantiate = tiles[activeTileset].wallTiles[0];


                        if (i == subdungeon.room.xMax - 1 && j == subdungeon.room.yMax - 1)
                            toInstantiate = tiles[activeTileset].wallTiles[1];
                        else if (i == subdungeon.room.xMax - 1 && j == subdungeon.room.y)
                            toInstantiate = tiles[activeTileset].wallTiles[3];
                        else if (i == subdungeon.room.x && j == subdungeon.room.y)
                            toInstantiate = tiles[activeTileset].wallTiles[5];
                        else if (i == subdungeon.room.x && j == subdungeon.room.yMax - 1)
                            toInstantiate = tiles[activeTileset].wallTiles[7];

                        CreateAt(i, j, toInstantiate);
                    }
                    else
                    {
                        if (i > subdungeon.room.x + 1 && i < subdungeon.room.xMax - 2
                        && j > subdungeon.room.y + 1 && j < subdungeon.room.yMax - 2)
                            freePositions.Add(new Vector2Int(i, j));
                        GameObject instance = CreateAt(i, j, toInstantiate);
                        RandomRotation(instance);
                    }
                }
            }
        }
        else
        {
            DrawRooms(subdungeon.left);
            DrawRooms(subdungeon.right);
        }
    }
    //Отрисовка основной части коридора
    private void DrawCorridors(SubDungeon subdungeon)
    {
        if (subdungeon == null)
        {
            return;
        }

        DrawCorridors(subdungeon.left);
        DrawCorridors(subdungeon.right);

        GameObject toInstantiate;

        foreach (Rect corridor in subdungeon.corridors)
        {
            for (int i = (int)corridor.x; i < corridor.xMax; i++)
            {
                for (int j = (int)corridor.y; j < corridor.yMax; j++)
                {
                    GameObject instance;
                    toInstantiate = RandomTile(tiles[activeTileset].floorTiles, 0.15f);
                    if (pathingPositions[i, j] != null)
                    {
                        if (pathingPositions[i, j].obj.CompareTag("Wall"))
                        {
                            instance = Replace(i, j, toInstantiate);
                            RandomRotation(instance);
                        }
                    }
                    else
                    {
                        instance = CreateAt(i, j, toInstantiate);
                        RandomRotation(instance);
                    }

                    CreateIfNotOccupied(i, j + 1, tiles[activeTileset].wallTiles[0]);
                    CreateIfNotOccupied(i, j - 1, tiles[activeTileset].wallTiles[4]);
                    CreateIfNotOccupied(i + 1, j, tiles[activeTileset].wallTiles[2]);
                    CreateIfNotOccupied(i - 1, j, tiles[activeTileset].wallTiles[6]);
                }
            }
        }
    }
    //Отрисовка частей коридора, соединяющихся с комнатами
    //Нужно потому, что, если рисовать весь коридор сразу, то другие коридоры, пересекающиеся с данным после, могут создать лишние стены
    private void FillInCorridorGaps(SubDungeon subdungeon)
    {
        if (subdungeon == null)
        {
            return;
        }

        FillInCorridorGaps(subdungeon.left);
        FillInCorridorGaps(subdungeon.right);

        foreach (Rect corridor in subdungeon.corridors)
        {
            for (int i = (int)corridor.x; i < corridor.xMax; i++)
            {
                for (int j = (int)corridor.y; j < corridor.yMax; j++)
                {
                    if (corridor.width > corridor.height)
                    {
                        if (pathingPositions[i, j + 1] != null && pathingPositions[i, j + 1].obj.CompareTag("Wall"))
                        {
                            PlaceAbove(i, j + 1, tiles[activeTileset].wallTiles[0]);
                        }
                        if (pathingPositions[i, j - 1] != null && pathingPositions[i, j - 1].obj.CompareTag("Wall"))
                        {
                            PlaceAbove(i, j - 1, tiles[activeTileset].wallTiles[4]);
                        }
                    }
                    else
                    {
                        if (pathingPositions[i + 1, j] != null && pathingPositions[i + 1, j].obj.CompareTag("Wall"))
                        {
                            PlaceAbove(i + 1, j, tiles[activeTileset].wallTiles[2]);
                        }
                        if (pathingPositions[i - 1, j] != null && pathingPositions[i - 1, j].obj.CompareTag("Wall"))
                        {
                            PlaceAbove(i - 1, j, tiles[activeTileset].wallTiles[6]);
                        }
                    }
                }
            }
        } 
    }
    //******************Отрисовка границ участков узлов для дебага******************
    private void OnDrawGizmos()
    {
        AttemptDebugDrawBsp();
    }

    private void OnDrawGizmosSelected()
    {
        AttemptDebugDrawBsp();
    }

    private void AttemptDebugDrawBsp()
    {
        if (shouldDebugDrawBSP)
        {
            DebugDrawBsp();
        }
    }

    private void DebugDrawBsp()
    {
        if (firstNode == null) return;

        DebugDrawBspNode(firstNode);
    }

    private void DebugDrawBspNode(SubDungeon node)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(node.rect.x, node.rect.y, 0), new Vector3(node.rect.xMax, node.rect.y, 0));
        Gizmos.DrawLine(new Vector3(node.rect.xMax, node.rect.y, 0), new Vector3(node.rect.xMax, node.rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(node.rect.x, node.rect.yMax, 0), new Vector3(node.rect.xMax, node.rect.yMax, 0));
        Gizmos.DrawLine(new Vector3(node.rect.x, node.rect.y, 0), new Vector3(node.rect.x, node.rect.yMax, 0));

        if (node.left != null) DebugDrawBspNode(node.left);
        if (node.right != null) DebugDrawBspNode(node.right);
    }
    //******************Конец отрисовки границ участков узлов для дебага************

    //Попытка поставить выход(неудачная в случае, если на случайно выбранной позиции нет верхней стены комнаты)
    private bool AttemptPlaceExit(SubDungeon leaf)
    {
        int randomPosition = (int)Random.Range(leaf.room.x + 2, leaf.room.xMax - 2);
        if (pathingPositions[randomPosition, (int)leaf.room.yMax - 1].obj.CompareTag("Wall")
            && pathingPositions[randomPosition + 1, (int)leaf.room.yMax - 1].obj.CompareTag("Wall")
            && pathingPositions[randomPosition - 1, (int)leaf.room.yMax - 1].obj.CompareTag("Wall"))
        {
            Replace(randomPosition, (int)leaf.room.yMax - 1, tiles[activeTileset].exitTile);

            PlaceAbove(randomPosition, (int)leaf.room.yMax - 1, tiles[activeTileset].exitArch);

            leaf.roomType = SubDungeon.RoomType.endRoom;
            return true;
        }
        return false;
    }
    //Поставить выход
    private void PlaceExit()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, leaves.Count - 1);
            if (leaves[randomIndex].roomType == SubDungeon.RoomType.regularRoom)
            {
                if (AttemptPlaceExit(leaves[randomIndex]))
                {
                    return;
                }
            }
        }
    }
    //Поставить игрока в самой маленькой комнате
    private void PlacePlayer()
    {
        SubDungeon smallestRoom = leaves[0];
        float smallestRoomSize = float.PositiveInfinity;
        foreach (SubDungeon leaf in leaves)
        {
            float leafRoomSize = leaf.room.width * leaf.room.height;
            if (leafRoomSize < smallestRoomSize)
            {
                smallestRoomSize = leafRoomSize;
                smallestRoom = leaf;
            }
        }

        smallestRoom.roomType = SubDungeon.RoomType.startRoom;
        Vector2 playerPos = new Vector2();
        playerPos.x = smallestRoom.room.x + (int)(smallestRoom.room.width / 2);
        playerPos.y = smallestRoom.room.y + (int)(smallestRoom.room.height / 2);
        Instantiate(tiles[activeTileset].playerTile, new Vector3(playerPos.x, playerPos.y, 0f), Quaternion.identity);
        int removeIndex = freePositions.FindIndex(v => (v.x == playerPos.x) && (v.y == playerPos.y));
        freePositions.RemoveAt(removeIndex);
    }
    //Выбрать случайную позицию из свободных и занять.
    //Флаг blockPath дополнительно делает место непроходимым.
    //Флаг spawnInStartRoom определяет можно ли выбрать позицию для установки объекта в стартовой комнате (чтобы в ней не появились враги).
    private Vector2Int RandomPosition(bool blockPath, bool spawnInStartRoom)
    {
        int randomIndex = Random.Range(0, freePositions.Count - 1);
        Vector2Int randomPosition = freePositions[randomIndex];
        if (blockPath)
        {
            pathingPositions[randomPosition.x, randomPosition.y].walkable = false;
        }
        if (!spawnInStartRoom)
        {
            while (!CanPlaceEnemy(randomPosition))
            {
                randomIndex = Random.Range(0, freePositions.Count - 1);
                randomPosition = freePositions[randomIndex];
            }
        }
        freePositions.RemoveAt(randomIndex);
        return randomPosition;
    }
    //Создает объекты случайного варианта, в случайном количестве, на случайной свободной позиции
    //Флаг blockPath дополнительно делает место непроходимым.
    //Флаг spawnInStartRoom определяет можно ли выбрать позицию для установки объекта в стартовой комнате (чтобы в ней не появились враги).
    private List<GameObject> LayoutObjectsAtRandom(GameObject[] tileArray, int minimum, int maximum, bool blockPath, bool spawnInStartRoom)
    {
        List<GameObject> obj = new List<GameObject>();
        int objectCount = Random.Range(minimum, maximum);
        Debug.Log("icount " + objectCount);
        for (int i = 0; i < objectCount; i++)
        {
            Vector2Int randomPosition = RandomPosition(blockPath, spawnInStartRoom);
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            GameObject instance = Instantiate(tileChoice, new Vector3(randomPosition.x, randomPosition.y, 0f), Quaternion.identity);
            obj.Add(instance);
            instance.gameObject.transform.SetParent(entityHolder);
        }
        return obj;
    }
    //Можно ли поставить врага? (Нельзя если стартовая комната)
    private bool CanPlaceEnemy(Vector2Int pos)
    {
        Rect startRoom = leaves.Find(leaf => leaf.roomType == SubDungeon.RoomType.startRoom).room;
        if (pos.x >= startRoom.x && pos.x <= startRoom.xMax && pos.y >= startRoom.y && pos.y <= startRoom.yMax)
        {
            return false;
        }
        return true;
    }
    //Вызывается при готовности скрипта, нужен для получения объекта с ссылками на префабы
    private void Awake()
    {
        tiles = gameObject.GetComponents<ObjectReferences>();
    }
    //Создание уровня
    public void SetupScene(int level)
    {
        activeTileset = Random.Range(0, 3);
        leaves = new List<SubDungeon>();
        dungeonHolder = new GameObject("DungeonHolder").transform;
        entityHolder = new GameObject("Entities").transform;
        SubDungeon rootSubdungeon = new SubDungeon(new Rect(0, 0, boardRows, boardColumns));
        freePositions = new List<Vector2Int>();
        CreateBSP(rootSubdungeon);
        rootSubdungeon.CreateRoom();
        firstNode = rootSubdungeon;
        pathingPositions = new PathingNode[boardRows, boardColumns];
        DrawRooms(rootSubdungeon);
        DrawCorridors(rootSubdungeon);
        FillInCorridorGaps(rootSubdungeon);
        PlacePlayer();
        PlaceExit();

        blocksCount = new Count(freePositions.Count / 12, freePositions.Count / 8);
        LayoutObjectsAtRandom(tiles[activeTileset].blockTiles, blocksCount.minimum, blocksCount.maximum, true, true);
        LayoutObjectsAtRandom(tiles[activeTileset].interactableTiles, interactablesCount.minimum, interactablesCount.maximum, false, false);
        int trapCount = (int)Mathf.Clamp(Mathf.Pow(1.8f, level), 20, freePositions.Count / 13f);
        LayoutObjectsAtRandom(tiles[activeTileset].trapTiles, trapCount, trapCount, true, false);
        int enemyCount = (int)Mathf.Clamp(Mathf.Pow(2f, level), 18, freePositions.Count / 10f);
        LayoutObjectsAtRandom(tiles[activeTileset].enemyTiles, enemyCount, enemyCount, false, false);
    }
}