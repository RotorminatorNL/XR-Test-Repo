using UnityEngine;

[System.Serializable]
public class PerlinNoise
{
    [HideInInspector] public float MinPerlinNoiseValue;
    [HideInInspector] public float MaxPerlinNoiseValue;

    [SerializeField, Range(0.001f, 1f)] private float xScale = 0.005f;
    [SerializeField] private float xCoordOffset = 0f;
    [Space(10)]
    [SerializeField, Range(0.001f, 1f)] private float zScale = 0.005f;
    [SerializeField] private float zCoordOffset = 0f;
    [Space(10)]
    [SerializeField, Min(0)] private float yScale = 100f;
    [SerializeField, Min(0)] private float scaleMultiplier = 2f;

    private float[,] perlinNoiseValues;
    private int xLength;
    private int zLength;

    public float[,] GetHeightValues(int xLength, int zLength, Vector3 flatAreaSettings)
    {
        perlinNoiseValues = new float[(xLength + 1), (zLength + 1)];
        this.xLength = xLength;
        this.zLength = zLength;

        GenerateHeightValues(xLength, zLength);
        GenerateMinMaxPerlinNoiseValues();
        ApplyScaleMultiplier();
        GenerateMinMaxPerlinNoiseValues();
        ApplyFlatAreaSettings(flatAreaSettings);

        return perlinNoiseValues;
    }

    private void GenerateHeightValues(int xLength, int zLength)
    {
        for (int z = 0; z <= zLength; z++)
        {
            for (int x = 0; x <= xLength; x++)
            {
                float perlinNoiseXCoord = x * xScale + xCoordOffset;
                float perlinNoiseZCoord = z * zScale + zCoordOffset;
                perlinNoiseValues[x, z] = Mathf.PerlinNoise(perlinNoiseXCoord, perlinNoiseZCoord);
            }
        }
    }

    private void GenerateMinMaxPerlinNoiseValues()
    {
        MinPerlinNoiseValue = float.MaxValue;
        MaxPerlinNoiseValue = float.MinValue;

        for (int z = 0; z <= zLength; z++)
        {
            for (int x = 0; x <= xLength; x++)
            {
                float value = perlinNoiseValues[x, z];
                if (value < MinPerlinNoiseValue) MinPerlinNoiseValue = value;
                if (value > MaxPerlinNoiseValue) MaxPerlinNoiseValue = value;
            }
        }
    }

    private void ApplyScaleMultiplier()
    {
        for (int z = 0; z <= zLength; z++)
        {
            for (int x = 0; x <= xLength; x++)
            {
                float valueInversed = Mathf.InverseLerp(MinPerlinNoiseValue, MaxPerlinNoiseValue, perlinNoiseValues[x, z]);
                perlinNoiseValues[x, z] *= Mathf.Pow(valueInversed, scaleMultiplier) * yScale;
            } 
        }
    }

    private void ApplyFlatAreaSettings(Vector3 flatAreaSettings)
    {
        float minGroundHeight = (MaxPerlinNoiseValue - MinPerlinNoiseValue) * flatAreaSettings.x + MinPerlinNoiseValue;
        float maxGroundHeight = (MaxPerlinNoiseValue - MinPerlinNoiseValue) * flatAreaSettings.z + MinPerlinNoiseValue;
        float groundHeight = (MaxPerlinNoiseValue - MinPerlinNoiseValue) * flatAreaSettings.y + MinPerlinNoiseValue;

        for (int z = 0; z <= zLength; z++)
        {
            for (int x = 0; x <= xLength; x++)
            {
                float value = perlinNoiseValues[x, z];
                if (value > maxGroundHeight)
                {
                    float valueInversed = Mathf.InverseLerp(maxGroundHeight, MaxPerlinNoiseValue, value);
                    float newValue = (MaxPerlinNoiseValue - groundHeight) * valueInversed + groundHeight;
                    perlinNoiseValues[x, z] = newValue;
                }
                else if (value < minGroundHeight)
                {
                    float valueInversed = Mathf.InverseLerp(MinPerlinNoiseValue, minGroundHeight, value);
                    float newValue = (groundHeight - MinPerlinNoiseValue) * valueInversed;
                    perlinNoiseValues[x, z] = newValue;
                }
                else
                {
                    perlinNoiseValues[x, z] = groundHeight;
                }
            }
        }
    }
}