using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class RoomData
{
    public float x;
    public float y;
    public float width;
    public float height;
}

[Serializable]
public class LeafData
{
    public RoomData room;
    public int roomType;
}

[Serializable]
public class PathingNodeData
{
    public string obj;
    public bool walkable;
}


[Serializable]
public class SaveData
{
    
}
