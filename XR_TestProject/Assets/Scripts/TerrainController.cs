using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ClickableCube;

[System.Serializable]
public class TerrainController : MonoBehaviour
{
    [SerializeField] private Terrain terrainMesh;
    [SerializeField] private int terrainSize = 1000;

    [Header("Grid settings")]
    [SerializeField] private int cellSize = 50;
    [SerializeField] private int cellWallSize = 4;
    [SerializeField] private int cellWallHeight = 20;
    [SerializeField] private int cellGroundHeight = 10;

    [Header("Clickable cells")]
    [SerializeField] private Transform clickableCubesParent;
    [SerializeField] private GameObject clickableCubePrefab;

    [Space(10), SerializeField] private PerlinNoise perlinNoise;
    private float[,] heightmap;

    [SerializeField, HideInInspector] private int gridSize;
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
        return 1f / cellWallHeight * cellGroundHeight;
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

            heightmap[coordX, coordZ] = 1f / cellWallHeight * cellGroundHeight;
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
                alphamaps[x, z, 0] = heightValue == 1f / cellWallHeight * cellGroundHeight ? 1 :1f / cellWallHeight * cellGroundHeight;
                alphamaps[x, z, 1] = heightValue == 1 ? 1 : 1f / cellWallHeight * cellGroundHeight;
            }
        }

        return alphamaps;
    }

    private void GenerateClickableCubes()
    {
        int gridClickableCubeLength = gridSize * 2;
        float newCellSize = cellSize / 2f;
        for (int z = 0; z <= gridClickableCubeLength; z++)
        {
            for (int x = 0; x <= gridClickableCubeLength; x++)
            {
                float[] gridAxes = new float[] {x, z};
                float[] cubeAxes = new float[2];
                Vector3 cubeScale = new(cellWallSize, 1, cellWallSize);

                for (int i = 0; i < gridAxes.Length; i++)
                {
                    cubeAxes[i] = (gridAxes[i] + (gridAxes[i] / (terrainSize - 1f))) * newCellSize;
                    if (gridAxes[i] != 0) cubeAxes[i] -= gridAxes[i] != gridClickableCubeLength ? 0.5f : 1f;

                    if (gridAxes[i] % 2 == 1) cubeScale = gridAxes[i] == x ? new Vector3(cellWallSize, 1, newCellSize) : new Vector3(newCellSize, 1, cellWallSize);
                }

                if (x % 2 == 1 && z % 2 == 1) cubeScale = new Vector3(newCellSize, 1, newCellSize);

                Vector3 cubeCoord = new(cubeAxes[1] + transform.position.z, cellWallHeight + 0.5f, cubeAxes[0] + transform.position.x);

                int heightValueX = Mathf.RoundToInt(cubeAxes[0] - 0.5f);
                int heightValueY = Mathf.RoundToInt(cubeAxes[1] - 0.5f);
                heightValueX = heightValueX >= terrainSize - 0.5f ? terrainSize - 1 : heightValueX;
                heightValueY = heightValueY >= terrainSize - 0.5f ? terrainSize - 1 : heightValueY;
                float heightValue = heightmap[heightValueX, heightValueY];
                TerrainState cubeState = heightValue == 1 ? TerrainState.Wall : TerrainState.Ground;

                GameObject cube = Instantiate(clickableCubePrefab, cubeCoord, Quaternion.identity, clickableCubesParent);
                cube.transform.localScale = cubeScale;
                cube.GetComponent<ClickableCube>().SetData(this, new Vector2Int(x, z), cubeState);
            }
        }
    }

    public void ChangeStateOfWalls(Vector2Int[] wallCenterCoords, bool automatic = true, TerrainState state = TerrainState.Wall)
    {
        heightmap ??= terrainMesh.terrainData.GetHeights(0, 0, terrainSize, terrainSize);

        for (int i = 0; i < wallCenterCoords.Length; i++)
        {
            Vector2Int[] wallCoords = GetWallCoords(wallCenterCoords[i]);
            for (int x = wallCoords[0].x; x < wallCoords[0].y; x++)
            {
                for (int z = wallCoords[1].x; z < wallCoords[1].y; z++)
                {
                    if (automatic)
                    {
                        if (heightmap[z, x] == 1f / cellWallHeight * cellGroundHeight) heightmap[z, x] = 1;
                        else heightmap[z, x] = 1f / cellWallHeight * cellGroundHeight;
                    }
                    else
                    {
                        if (state == TerrainState.Wall) heightmap[z, x] = 1;
                        else if (state == TerrainState.Ground) heightmap[z, x] = 1f / cellWallHeight * cellGroundHeight;
                        else heightmap[z, x] = 0;
                    }
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
        float newCellSize = cellSize / 2f;

        for (int i = 0; i < wallCoords.Length; i++)
        {
            int flooredCoord = Mathf.FloorToInt(wallCenterCoords[i] * newCellSize);

            wallCoords[i].x = -cellWallSize + flooredCoord;
            wallCoords[i].y = cellWallSize + flooredCoord;

            if (wallCenterCoords[i] % 2 == 1)
            {
                wallCoords[i].x = flooredCoord - (Mathf.FloorToInt(cellSize / 2f) - cellWallSize);
                wallCoords[i].y = flooredCoord + (Mathf.CeilToInt(cellSize / 2f) - cellWallSize);
            }

            if (wallCenterCoords[i] == 0) wallCoords[i].x = 0;
            if (wallCenterCoords[i] == gridClickableCubeLength) wallCoords[i].y = flooredCoord;
        }

        return wallCoords;
    }
}