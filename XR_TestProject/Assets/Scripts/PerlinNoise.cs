using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
}