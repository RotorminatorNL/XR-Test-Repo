using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [SerializeField] private Terrain terrainMesh;
    [SerializeField] private int terrainSize = 1000;

    [Header("Grid settings")]
    [SerializeField] private int cellSize = 50;
    [SerializeField] private int cellWallSize = 4;
    [SerializeField] private int cellWallHeight = 20;

    [Header("Clickable cells")]
    [SerializeField] private Transform clickableCubesParent;
    [SerializeField] private GameObject clickableCubePrefab;

    [Space(10), SerializeField] private PerlinNoise perlinNoise;

    private int gridSize;
    private readonly int cellWallAmount = 4; 
    private float[,] heightmap;

    [SerializeField, HideInInspector] private CellDataShared cellDataShared;

    public void GenerateTerrain()
    {
        TerrainData terrainData = terrainMesh.terrainData;
        terrainData.heightmapResolution = terrainSize;
        terrainSize = terrainData.heightmapResolution;

        terrainData.alphamapResolution = terrainSize;
        terrainData.size = new Vector3(terrainSize, cellWallHeight, terrainSize);

        gridSize = terrainSize / cellSize;
        heightmap = new float[terrainSize, terrainSize];

        cellDataShared = new CellDataShared
        {
            CellsOriginalHeight = new float[cellSize, cellSize],
            TopWallCoords = new List<string>(),
            BottomWallCoords = new List<string>(),
            LeftWallCoords = new List<string>(),
            RightWallCoords = new List<string>()
        };

        for (int i = clickableCubesParent.childCount - 1; i >= 0; i--) DestroyImmediate(clickableCubesParent.GetChild(i).gameObject);

        GenerateHeightValues();
        GenerateMaze();
        FlushChanges();
        GenerateClickableCubes();
    }

    private void GenerateHeightValues()
    {
        bool dataCollected = false;

        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int cellZLength = 0; cellZLength < cellSize; cellZLength++)
                {
                    for (int cellXLength = 0; cellXLength < cellSize; cellXLength++)
                    {
                        int newX = x * cellSize + cellXLength;
                        int newZ = z * cellSize + cellZLength;
                        float heightValue = GetHeightValue(cellXLength, cellZLength);
                        heightmap[newX, newZ] = heightValue;

                        if (!dataCollected) SetCellDataShare(cellXLength, cellZLength, heightValue);
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
        List<Vector2Int> visitedCells = new() { Vector2Int.zero };

        bool[,] cellsInsideMaze = new bool[gridSize, gridSize];
        cellsInsideMaze[0, 0] = true;

        for (int i = 0; i < cellsInsideMaze.Length; i++)
        {
            for (int x = visitedCells.Count - 1; x >= 0; x--)
            {
                Vector2Int currentCoord = visitedCells[x];
                Vector2Int nextCoord = GetNextCoord(currentCoord, cellsInsideMaze);

                if (nextCoord == Vector2Int.zero) continue;

                ChangeStateOfWallBetweenCells(currentCoord, nextCoord);
                visitedCells.Add(nextCoord);
                cellsInsideMaze[nextCoord.x, nextCoord.y] = true;
                break;
            }
        }
    }

    private Vector2Int GetNextCoord(Vector2Int currentCoord, bool[,] cellsInsideMaze)
    {
        Vector2Int[] nextCoords = new Vector2Int[4];
        nextCoords[0] = new(currentCoord.x, currentCoord.y + 1);
        nextCoords[1] = new(currentCoord.x, currentCoord.y - 1);
        nextCoords[2] = new(currentCoord.x - 1, currentCoord.y);
        nextCoords[3] = new(currentCoord.x + 1, currentCoord.y);

        float currentHeight = 0;
        Vector2Int nextCoord = Vector2Int.zero;

        for (int i = 0; i < nextCoords.Length; i++)
        {
            float nextCoordHeight = perlinNoise.GetPerlinNoiseValue(nextCoords[i]);
            if (IsNextCoordPossible(nextCoords[i], cellsInsideMaze) && nextCoordHeight > currentHeight)
            {
                currentHeight = nextCoordHeight;
                nextCoord = nextCoords[i];
            }
        }

        return nextCoord;
    }

    private bool IsNextCoordPossible(Vector2Int nextCoord, bool[,] cellsInsideMaze)
    {
        if (nextCoord.x < 0 || nextCoord.x >= gridSize || nextCoord.y < 0 | nextCoord.y >= gridSize) return false;
        return !cellsInsideMaze[nextCoord.x, nextCoord.y];
    }

    private void ChangeStateOfWallBetweenCells(Vector2Int cellCoord_1, Vector2Int cellCoord_2)
    {
        ChangeStateOfCellWall(cellCoord_1, cellDataShared.GetWallCoords(GetWallDirection(cellCoord_1, cellCoord_2)));
        ChangeStateOfCellWall(cellCoord_2, cellDataShared.GetWallCoords(GetWallDirection(cellCoord_2, cellCoord_1)));
    }

    private CellDataShared.WallDirection GetWallDirection(Vector2 cellCoord_1, Vector2 cellCoord_2)
    {
        if (cellCoord_1.x != cellCoord_2.x) return cellCoord_1.x < cellCoord_2.x ? CellDataShared.WallDirection.Top : CellDataShared.WallDirection.Bottom;
        else return cellCoord_1.y > cellCoord_2.y ? CellDataShared.WallDirection.Left : CellDataShared.WallDirection.Right;
    }

    private void ChangeStateOfCellWall(Vector2Int cellCoord, List<string> cellWallCoords)
    {
        for (int coordIndex = 0; coordIndex < cellWallCoords.Count; coordIndex++)
        {
            string[] coordArray = cellWallCoords[coordIndex].Split(',');
            int coordX = int.Parse(coordArray[0]) + cellSize * cellCoord.x;
            int coordZ = int.Parse(coordArray[1]) + cellSize * cellCoord.y;

            heightmap[coordX, coordZ] = 0;
        }
    }

    private void FlushChanges()
    {
        terrainMesh.terrainData.SetHeights(0, 0, heightmap);
        terrainMesh.terrainData.SetAlphamaps(0, 0, GetAlphamaps());
        terrainMesh.Flush();
    }

    private float[,,] GetAlphamaps()
    {
        float[,,] alphamaps = new float[terrainSize, terrainSize, terrainMesh.terrainData.terrainLayers.Length];

        if (terrainMesh.terrainData.terrainLayers.Length == 0) return alphamaps;

        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                for (int i = 0; i < terrainMesh.terrainData.terrainLayers.Length; i++) alphamaps[x, z, i] = 0f;

                float heightValue = heightmap[x, z];
                alphamaps[x, z, 0] = heightValue == 0 ? 1 : 0;
                alphamaps[x, z, 1] = heightValue == 1 ? 1 : 0;
            }
        }

        return alphamaps;
    }

    private void GenerateClickableCubes()
    {
        int gridClickableCubeLength = gridSize * 2;
        int newCellSize = cellSize / 2;
        for (int z = 0; z <= gridClickableCubeLength; z++)
        {
            for (int x = 0; x <= gridClickableCubeLength; x++)
            {
                if (x % 2 == 1 && z % 2 == 1) continue;

                float newX = x * newCellSize + transform.position.z;
                float newZ = z * newCellSize + transform.position.x;
                Vector3 newScale = new(newCellSize / 5, 1, newCellSize / 5);

                int gridX = x < gridClickableCubeLength ? x * newCellSize : x * newCellSize - 1;
                int gridZ = z < gridClickableCubeLength ? z * newCellSize : z * newCellSize - 1;
                bool state = heightmap[gridX, gridZ] == 1;

                if (x % 2 == 1 || z % 2 == 1)
                {
                    newX = x * newCellSize + transform.position.z;
                    newZ = z * newCellSize + transform.position.x;

                    newScale = new Vector3(newCellSize / 4, 1, newCellSize);
                    if (z % 2 == 1) newScale = new Vector3(newCellSize, 1, newCellSize / 4);
                }

                GameObject cube = Instantiate(clickableCubePrefab, new(newZ, cellWallHeight + 2, newX), Quaternion.identity, clickableCubesParent);
                cube.transform.localScale = newScale;
                cube.GetComponent<ClickableCube>().SetData(this, new Vector2Int(x, z), state);
            }
        }
    }

    public void ChangeStateOfWalls(Vector2Int[] wallCenterCoord, bool automatic = true, bool state = false)
    {
        heightmap ??= terrainMesh.terrainData.GetHeights(0, 0, terrainSize, terrainSize);

        for (int i = 0; i < wallCenterCoord.Length; i++)
        {
            Vector2Int[] wallCoords = GetWallCoords(wallCenterCoord[i]);
            for (int x = wallCoords[0].x; x < wallCoords[0].y; x++)
            {
                for (int z = wallCoords[1].x; z < wallCoords[1].y; z++)
                {
                    if (automatic) heightmap[z, x] = 1 - heightmap[z, x];
                    else heightmap[z, x] = state ? 1 : 0;
                }
            }
        }

        FlushChanges();
    }

    private Vector2Int[] GetWallCoords(Vector2Int wallCenterCoord)
    {
        Vector2Int[] wallCoords = new Vector2Int[2];
        int[] wallCenterCoords = new int[] { wallCenterCoord.y, wallCenterCoord.x };

        int gridClickableCubeLength = gridSize * 2;
        int newCellSize = cellSize / 2;

        for (int i = 0; i < wallCoords.Length; i++)
        {
            wallCoords[i].x = -cellWallSize + wallCenterCoords[i] * newCellSize;
            wallCoords[i].y = cellWallSize + wallCenterCoords[i] * newCellSize;

            if (wallCenterCoords[i] % 2 == 1)
            {
                wallCoords[i].x = wallCenterCoords[i] * newCellSize - ((cellSize / 2) - cellWallSize);
                wallCoords[i].y = wallCenterCoords[i] * newCellSize + ((cellSize / 2) - cellWallSize);
            }

            if (wallCenterCoords[i] == 0) wallCoords[i].x = 0;
            if (wallCenterCoords[i] == gridClickableCubeLength) wallCoords[i].y = wallCenterCoords[i] * newCellSize;
        }

        return wallCoords;
    }
}