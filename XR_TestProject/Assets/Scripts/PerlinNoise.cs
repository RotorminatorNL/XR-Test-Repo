using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class PerlinNoise
{
    [SerializeField, Range(0.001f, 1f)] private float xScale = 0.005f;
    [SerializeField] private float xCoordOffset = 0f;
    [Space(10)]
    [SerializeField, Range(0.001f, 1f)] private float zScale = 0.005f;
    [SerializeField] private float zCoordOffset = 0f;

    public float GetPerlinNoiseValue(Vector2 coord)
    {
        float perlinNoiseXCoord = coord.x * xScale + xCoordOffset;
        float perlinNoiseZCoord = coord.y * zScale + zCoordOffset;

        return Mathf.PerlinNoise(perlinNoiseXCoord, perlinNoiseZCoord);
    }
}