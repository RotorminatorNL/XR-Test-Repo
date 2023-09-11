using UnityEngine;

[System.Serializable]
public class PerlinNoise
{
    [HideInInspector] public float MinPerlinNoiseValue = 0;
    [HideInInspector] public float MaxPerlinNoiseValue => yScale;

    [SerializeField, Range(0.001f, 1f)] private float xScale = 0.005f;
    [SerializeField] private float xCoordOffset = 0f;
    [Space(10)]
    [SerializeField, Range(0.001f, 1f)] private float zScale = 0.005f;
    [SerializeField] private float zCoordOffset = 0f;
    [Space(10)]
    [SerializeField, Min(0)] private float yScale = 100f;

    private float[,] perlinNoiseValues;
    private int xLength;
    private int zLength;

    private int gridXLength;
    private int gridZLength;
    private int cellSize;
    private int cellWallSize;
    private int slopeAngle;

    public float[,] GetHeightValues(int xLength, int zLength, int cellSize, int cellWallSize, int slopeAngle)
    {
        perlinNoiseValues = new float[xLength, zLength];
        this.xLength = xLength;
        this.zLength = zLength;

        gridXLength = xLength / cellSize;
        gridZLength = zLength / cellSize;
        this.cellSize = cellSize;
        this.cellWallSize = cellWallSize;
        this.slopeAngle = slopeAngle;

        GenerateHeightValues();

        return perlinNoiseValues;
    }

    private void GenerateHeightValues()
    {
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

                        perlinNoiseValues[newX, newZ] = GetHeightValue(cellXLength, cellZLength);
                    }
                }
            }
        }
    }

    private float GetHeightValue(int x, int z)
    {
        int cellBottomLeftStart = cellWallSize;
        int cellBottomLeftEnd = cellWallSize + slopeAngle;
        int cellTopRightStart = cellSize - cellWallSize - slopeAngle;
        int cellTopRightEnd = cellSize - cellWallSize;

        if (x < cellBottomLeftStart || x >= cellTopRightEnd || z < cellBottomLeftStart || z >= cellTopRightEnd) return 1;

        float result = 0;

        if (z < cellBottomLeftEnd) 
        {
            result = 1f - Mathf.InverseLerp(cellBottomLeftStart - 1, cellBottomLeftEnd, z);

            float newResult = 0;
            if (x < cellBottomLeftEnd) newResult = 1f - Mathf.InverseLerp(cellBottomLeftStart - 1, cellBottomLeftEnd, x);
            if (x >= cellTopRightStart) newResult = Mathf.InverseLerp(cellTopRightStart - 1, cellTopRightEnd, x);
            result = newResult >= result ? newResult : result;
        }
        else if (z >= cellTopRightStart)
        {
            result = Mathf.InverseLerp(cellTopRightStart - 1, cellTopRightEnd, z);

            float newResult = 0;
            if (x < cellBottomLeftEnd) newResult = 1f - Mathf.InverseLerp(cellBottomLeftStart - 1, cellBottomLeftEnd, x);
            if (x >= cellTopRightStart) newResult = Mathf.InverseLerp(cellTopRightStart - 1, cellTopRightEnd, x);
            result = newResult >= result ? newResult : result;
        }
        else if (x < cellBottomLeftEnd)
        {
            result = 1f - Mathf.InverseLerp(cellBottomLeftStart - 1, cellBottomLeftEnd, x);
        }
        else if (x >= cellTopRightStart)
        {   
            result = Mathf.InverseLerp(cellTopRightStart - 1, cellTopRightEnd, x);
        }

        return result;
    }
}