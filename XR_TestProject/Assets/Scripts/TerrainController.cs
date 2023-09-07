using UnityEngine;

[System.Serializable]
public class TerrainController : MonoBehaviour
{
    //[SerializeField] private MeshGenerator meshGenerator;
    [SerializeField] private Terrain terrainMesh;
    [SerializeField] private Vector2Int terrainSize = new(1000, 1000);

    [Header("Flat area settings")]
    [SerializeField, Range(0f, 1f)] private float groundHeightPercent = 0.02f;
    [SerializeField, Range(0f, 1f)] private float minBetweenPercent = 0.02f;
    [SerializeField, Range(0f, 1f)] private float maxBetweenPercent = 0.25f;
    [Space(10)]
    [SerializeField] private PerlinNoise perlinNoise;
    [Space(10)]
    [SerializeField] private Gradient gradient;

    private int xLength;
    private int zLength;

    public void GenerateTerrain()
    {
        Vector3 flatAreaSettings = new(minBetweenPercent, groundHeightPercent, maxBetweenPercent);
        //meshGenerator.Generate(terrainSize, flatAreaSettings, gradient, perlinNoise);

        xLength = terrainSize.x;
        zLength = terrainSize.y;

        float[,] heightmap = GetHeightmap(perlinNoise.GetHeightValues(xLength, zLength, flatAreaSettings));

        terrainMesh.terrainData.size = new Vector3(xLength, perlinNoise.MaxPerlinNoiseValue, zLength);
        terrainMesh.terrainData.heightmapResolution = xLength;
        terrainMesh.terrainData.SetHeights(0, 0, heightmap);
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