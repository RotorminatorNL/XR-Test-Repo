using UnityEngine;

[System.Serializable]
public class TerrainLayerData
{
    [SerializeField] private int terrainLayerArrayIndex;
    [SerializeField] private int terrainLayerArrayLength;
    [SerializeField] private string terrainLayerName;
    [Range(0f, 1f)] public float HeightPosition;
    [Range(0f, 1f)] public float BlendPercent;

    public TerrainLayerData(int arrayIndex, int arrayLength, string terrainLayerName, float heightPosition, float blendPercent) 
    {
        terrainLayerArrayIndex = arrayIndex;
        terrainLayerArrayLength = arrayLength;
        this.terrainLayerName = terrainLayerName;
        HeightPosition = heightPosition;
        BlendPercent = blendPercent;
    }
}
