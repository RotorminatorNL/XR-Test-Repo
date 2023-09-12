using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainController : MonoBehaviour
{
    [SerializeField] private Terrain terrainMesh;
    [SerializeField] private int terrainSize = 1000;

    [Header("Grid settings")]
    [SerializeField] private int cellSize = 50;
    [SerializeField] private int cellWallSize = 4;
    [SerializeField] private int cellWallHeight = 20;

    [Space(10)]
    [SerializeField] private PerlinNoise perlinNoise;

    private int xLength;
    private int zLength;

    private float[,] heightmap;

    private int gridXLength;
    private int gridZLength;

    [SerializeField, HideInInspector] private CellDataShared cellDataShared;
    [SerializeField, HideInInspector] private bool[,] cellsInsideMaze;

    public void GenerateTerrain()
    {
        TerrainData terrainData = terrainMesh.terrainData;

        terrainData.heightmapResolution = terrainSize;
        xLength = terrainData.heightmapResolution;
        zLength = terrainData.heightmapResolution;

        GenerateHeightValues();
        GenerateMaze();

        terrainData.size = new Vector3(xLength, cellWallHeight, zLength);
        terrainData.alphamapResolution = xLength;
        terrainData.SetHeights(0, 0, heightmap);
        terrainData.SetAlphamaps(0, 0, GetAlphamaps());
        terrainMesh.Flush();
    }

    private void GenerateHeightValues()
    {
        heightmap = new float[xLength, zLength];
        gridXLength = xLength / cellSize;
        gridZLength = zLength / cellSize;

        cellDataShared = new CellDataShared
        {
            CellsOriginalHeight = new float[cellSize, cellSize],
            TopWallCoords = new List<string>(),
            BottomWallCoords = new List<string>(),
            LeftWallCoords = new List<string>(),
            RightWallCoords = new List<string>()
        };

        bool dataCollected = false;

        cellsInsideMaze = new bool[gridXLength, gridZLength];

        for (int z = 0; z < gridZLength; z++)
        {
            for (int x = 0; x < gridXLength; x++)
            {
                for (int cellZLength = 0; cellZLength < cellSize; cellZLength++)
                {
                    for (int cellXLength = 0; cellXLength < cellSize; cellXLength++)
                    {
                        int newX = x * cellSize + cellXLength;
                        int newZ = z * cellSize + cellZLength;

                        float heightValue = GetHeightValue(cellXLength, cellZLength);

                        if (!dataCollected) SetCellDataShare(cellXLength, cellZLength, heightValue);

                        heightmap[newX, newZ] = heightValue;
                    }
                }

                dataCollected = true;
            }
        }
    }

    private float GetHeightValue(int x, int z)
    {
        if (x < cellWallSize || x >= cellSize - cellWallSize || z < cellWallSize || z >= cellSize - cellWallSize) return 1;
        return 0;
    }

    private void SetCellDataShare(int x, int z, float heightValue)
    {
        if (z >= cellWallSize && z < cellSize - cellWallSize)
        {
            if (x >= cellSize - cellWallSize && x < cellSize) cellDataShared.TopWallCoords.Add($"{x},{z}");
            if (x >= 0 && x < cellWallSize) cellDataShared.BottomWallCoords.Add($"{x},{z}");
        }
        if (x >= cellWallSize && x < cellSize - cellWallSize)
        {
            if (z >= 0 && z < cellWallSize) cellDataShared.LeftWallCoords.Add($"{x},{z}");
            if (z >= cellSize - cellWallSize && z < cellSize) cellDataShared.RightWallCoords.Add($"{x},{z}");
        }

        cellDataShared.CellsOriginalHeight[x, z] = heightValue;
    }

    private void GenerateMaze()
    {
        List<Vector2Int> cells = new();
        int cellsAmount = 0;

        Vector2Int currentCoord = Vector2Int.zero;

        cellsInsideMaze[0, 0] = true;

        for (int i = 0; i < cellsInsideMaze.Length; i++)
        {
            Vector2Int topNextPos = new(currentCoord.x, currentCoord.y + 1);
            Vector2Int bottomNextPos = new(currentCoord.x, currentCoord.y - 1);
            Vector2Int leftNextPos = new(currentCoord.x - 1, currentCoord.y);
            Vector2Int rightNextPos = new(currentCoord.x + 1, currentCoord.y);

            bool topAvailable = topNextPos.y < gridZLength && !cellsInsideMaze[topNextPos.x, topNextPos.y];
            bool bottomAvailable = bottomNextPos.y >= 0 && !cellsInsideMaze[bottomNextPos.x, bottomNextPos.y];
            bool leftAvailable = leftNextPos.x >= 0 && !cellsInsideMaze[leftNextPos.x, leftNextPos.y];
            bool rightAvailable = rightNextPos.x < gridXLength && !cellsInsideMaze[rightNextPos.x, rightNextPos.y];
            
            if (!topAvailable && !bottomAvailable && !leftAvailable && !rightAvailable)
            {
                for (int x = cellsAmount - 1; x > 0; x--)
                {
                    currentCoord = cells[x];

                    topNextPos = new(currentCoord.x, currentCoord.y + 1);
                    bottomNextPos = new(currentCoord.x, currentCoord.y - 1);
                    leftNextPos = new(currentCoord.x - 1, currentCoord.y);
                    rightNextPos = new(currentCoord.x + 1, currentCoord.y);

                    topAvailable = topNextPos.y < gridZLength && !cellsInsideMaze[topNextPos.x, topNextPos.y];
                    bottomAvailable = bottomNextPos.y >= 0 && !cellsInsideMaze[bottomNextPos.x, bottomNextPos.y];
                    leftAvailable = leftNextPos.x >= 0 && !cellsInsideMaze[leftNextPos.x, leftNextPos.y];
                    rightAvailable = rightNextPos.x < gridXLength && !cellsInsideMaze[rightNextPos.x, rightNextPos.y];

                    if (topAvailable || bottomAvailable || leftAvailable || rightAvailable) break;
                }
            }

            float topHeight = perlinNoise.GetPerlinNoiseValue(topNextPos);
            float bottomHeight = perlinNoise.GetPerlinNoiseValue(bottomNextPos);
            float leftHeight = perlinNoise.GetPerlinNoiseValue(leftNextPos);
            float rightHeight = perlinNoise.GetPerlinNoiseValue(rightNextPos);

            float currentHeight = 0;
            Vector2Int currentNextCoord = Vector2Int.zero;
            
            if (topAvailable)
            {
                currentHeight = topHeight;
                currentNextCoord = topNextPos;
            }
            if (bottomAvailable && bottomHeight > currentHeight)
            {
                currentHeight = bottomHeight;
                currentNextCoord = bottomNextPos;
            }
            if (leftAvailable && leftHeight > currentHeight)
            {
                currentHeight = leftHeight;
                currentNextCoord = leftNextPos;
            }
            if (rightAvailable && rightHeight > currentHeight)
            {
                currentNextCoord = rightNextPos;
            }

            RemoveWallBetweenCells(currentCoord, currentNextCoord);

            cells.Add(currentNextCoord);
            cellsAmount = cells.Count;

            currentCoord = currentNextCoord;
            cellsInsideMaze[currentCoord.x, currentCoord.y] = true;
        }
    }

    private void RemoveWallBetweenCells(Vector2Int cellCoord_1, Vector2Int cellCoord_2)
    {
        CellDataShared.WallDirection cellWall_1 = GetWallDirection(cellCoord_1, cellCoord_2);
        RemoveWallPart(cellCoord_1, cellDataShared.GetWallCoords(cellWall_1));

        CellDataShared.WallDirection cellWall_2 = GetWallDirection(cellCoord_2, cellCoord_1);
        RemoveWallPart(cellCoord_2, cellDataShared.GetWallCoords(cellWall_2));
    }

    private CellDataShared.WallDirection GetWallDirection(Vector2 cellCoord_1, Vector2 cellCoord_2)
    {
        CellDataShared.WallDirection wallDirection;
        if (cellCoord_1.x != cellCoord_2.x) wallDirection = cellCoord_1.x < cellCoord_2.x ? CellDataShared.WallDirection.Top : CellDataShared.WallDirection.Bottom;
        else wallDirection = cellCoord_1.y > cellCoord_2.y ? CellDataShared.WallDirection.Left : CellDataShared.WallDirection.Right;

        return wallDirection;
    }

    private void RemoveWallPart(Vector2Int cellCoord, List<string> wallCoords)
    {
        for (int coordIndex = 0; coordIndex < wallCoords.Count; coordIndex++)
        {
            string[] coordArray = wallCoords[coordIndex].Split(',');
            int coordX = int.Parse(coordArray[0]) + cellSize * cellCoord.x;
            int coordZ = int.Parse(coordArray[1]) + cellSize * cellCoord.y;

            heightmap[coordX, coordZ] = 0;
        }
    }

    private float[,,] GetAlphamaps()
    {
        TerrainLayer[] terrainLayers = terrainMesh.terrainData.terrainLayers;
        float[,,] alphamaps = new float[xLength, zLength, terrainLayers.Length];

        for (int z = 0; z < zLength; z++)
        {
            for (int x = 0; x < xLength; x++)
            {
                for (int i = 0; i < terrainLayers.Length; i++) alphamaps[x, z, i] = 0f;

                float heightValue = heightmap[x, z];
                alphamaps[x, z, 0] = heightValue == 0 ? 1 : 0;
                alphamaps[x, z, 1] = heightValue == 1 ? 1 : 0;
            }
        }

        return alphamaps;
    }
}