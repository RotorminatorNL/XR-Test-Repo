using UnityEngine;

[System.Serializable]
public class TerrainController : MonoBehaviour
{
    //[SerializeField] private MeshGenerator meshGenerator;
    [SerializeField] private Terrain terrainMesh;
    [SerializeField] private int terrainSize = 1000;

    [Header("Flat area settings")]
    [SerializeField, Range(0f, 1f)] private float groundHeightPercent = 0.02f;
    [SerializeField, Range(0f, 1f)] private float minBetweenPercent = 0.02f;
    [SerializeField, Range(0f, 1f)] private float maxBetweenPercent = 0.25f;
    [Space(10)]
    [SerializeField] private PerlinNoise perlinNoise;
    [Space(10)]
    [SerializeField, Range(0f,1f)] private float[] terrainLayerHeightPos;

    private int xLength;
    private int zLength;

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

        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int i = 0; i < terrainData.terrainLayers.Length; i++) alphaMap[x, y, i] = 0f;

                float heightValue = heightmap[x, y];

                for (int i = 0; i < terrainLayerHeightPos.Length; i++)
                {
                    if (terrainLayerHeightPos[i] >= heightValue)
                    {
                        alphaMap[x, y, i] = 1f;
                        break;
                    }
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
        terrainMesh.Flush();
    }

    private float[,] GetHeightmap(float[,] heightValues)
    {
        float[,] result = new float[xLength, zLength];

        for (int z = 0; z < zLength; z++)
        {
            for (int x = 0; x < xLength; x++)
            {
                result[x,z] = Mathf.InverseLerp(perlinNoise.MinPerlinNoiseValue, perlinNoise.MaxPerlinNoiseValue, heightValues[x,z]);
            }
        }

        return result;
    }
}