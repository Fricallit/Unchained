using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class EntityData
{
    public int arrayIndex;
    public float x;
    public float y;
}


[Serializable]
public class SaveData
{
    public int worldSeed;
    public int playerHealth;
    public int score;
    public int level;
    public float playerX;
    public float PlayerY;

    public List<EntityData> enemyData;
    public List<EntityData> interactablesData;
}
