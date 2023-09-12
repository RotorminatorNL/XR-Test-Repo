using System.Collections.Generic;

[System.Serializable]
public class CellData
{

}

[System.Serializable]
public class CellDataShared
{
    public enum WallDirection { Top, Bottom, Left, Right }

    public float[,] CellsOriginalHeight;

    public List<string> TopWallCoords;
    public List<string> BottomWallCoords;
    public List<string> LeftWallCoords;
    public List<string> RightWallCoords;

    public List<string> GetWallCoords(WallDirection wall)
    {
        if (wall == WallDirection.Top) { return TopWallCoords; }
        if (wall == WallDirection.Bottom) { return BottomWallCoords; }
        if (wall == WallDirection.Left) { return LeftWallCoords; }
        if (wall == WallDirection.Right) { return RightWallCoords; }
        return null;
    }
}
