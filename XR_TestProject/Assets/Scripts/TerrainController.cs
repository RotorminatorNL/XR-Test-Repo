using UnityEngine;

[System.Serializable]
public class TerrainController : MonoBehaviour
{
    [SerializeField] private Terrain terrainMesh;
    [SerializeField] private int terrainSize = 1000;

    [Header("Flat area settings")]
    [SerializeField, Range(0f, 1f)] private float groundHeightPercent = 0.02f;
    [SerializeField, Range(0f, 1f)] private float minBetweenPercent = 0.02f;
    [SerializeField, Range(0f, 1f)] private float maxBetweenPercent = 0.25f;
    [Space(10)]
    [SerializeField] private PerlinNoise perlinNoise;
    [Space(10)]
    [SerializeField] private TerrainLayerData[] terrainLayerData;

    private int xLength;
    private int zLength;

    public void ResetTerrainLayerData()
    {
        TerrainLayer[] terrainLayers = terrainMesh.terrainData.terrainLayers;

        if (terrainLayers.Length != terrainLayerData.Length)
        {
            terrainLayerData = new TerrainLayerData[terrainLayers.Length];
            for (int i = 0; i < terrainLayers.Length; i++) terrainLayerData[i] = new TerrainLayerData(i, terrainLayerData.Length, terrainLayers[i].name, 0f, 0f);
        }
    }

    public void GenerateTerrain()
    {
        TerrainData terrainData = terrainMesh.terrainData;
        Vector3 flatAreaSettings = new(minBetweenPercent, groundHeightPercent, maxBetweenPercent);

        terrainData.heightmapResolution = terrainSize;
        xLength = terrainData.heightmapResolution;
        zLength = terrainData.heightmapResolution;

        float[,] heightmap = GetHeightmap(perlinNoise.GetHeightValues(xLength, zLength, flatAreaSettings));

        terrainData.size = new Vector3(xLength, perlinNoise.MaxPerlinNoiseValue, zLength);
        terrainData.alphamapResolution = xLength;
        terrainData.SetHeights(0, 0, heightmap);

        terrainData.SetAlphamaps(0, 0, GetAlphamap(terrainData, heightmap));
        terrainMesh.Flush();
    }

    private float[,] GetHeightmap(float[,] heightValues)
    {
        float[,] result = new float[xLength, zLength];

        for (int z = 0; z < zLength; z++)
        {
            for (int x = 0; x < xLength; x++)
            {
                result[x, z] = Mathf.InverseLerp(perlinNoise.MinPerlinNoiseValue, perlinNoise.MaxPerlinNoiseValue, heightValues[x, z]);
            }
        }

        return result;
    }

    private float[,,] GetAlphamap(TerrainData terrainData, float[,] heightmap)
    {
        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int i = 0; i < terrainData.terrainLayers.Length; i++) alphaMap[x, y, i] = 0f;

                float heightValue = heightmap[x, y];

                for (int i = 0; i < terrainLayerData.Length; i++)
                {
                    if (i == 0 && terrainLayerData[i].HeightPosition >= heightValue)
                    {
                        alphaMap[x, y, i] = 1f;
                        break;
                    }
                    else if (i + 1 == terrainLayerData.Length && terrainLayerData[i - 1].HeightPosition < heightValue) break;
                    else if (terrainLayerData[i].HeightPosition < heightValue && terrainLayerData[i + 1].HeightPosition >= heightValue) 
                    {
                        float tempHeight1 = terrainLayerData[i].HeightPosition;
                        float tempHeight2 = terrainLayerData[i + 1].HeightPosition;
                        float heightDifference = tempHeight2 - tempHeight1;
                        float minBlendHeight = tempHeight1;
                        float maxBlendHeight = tempHeight1 + (heightDifference * terrainLayerData[i].BlendPercent);

                        if (minBlendHeight != maxBlendHeight)
                        {
                            float alpha = Mathf.InverseLerp(minBlendHeight, maxBlendHeight, heightValue);
                            alphaMap[x, y, i] = 1 - alpha;
                            alphaMap[x, y, i + 1] = alpha;
                            break;
                        }

                        alphaMap[x, y, i] = 0;
                        alphaMap[x, y, i + 1] = 1;
                        break;
                    }
                }
            }
        }

        return alphaMap;
    }
}