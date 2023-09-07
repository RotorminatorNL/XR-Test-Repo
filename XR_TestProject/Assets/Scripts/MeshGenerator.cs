using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;

    private PerlinNoise perlinNoise;
    private int xLength;
    private int zLength;

    public void Generate(Vector2 terrainSize, Vector3 flatAreaSettings, Gradient gradient, PerlinNoise perlinNoise)
    {
        this.perlinNoise = perlinNoise;
        xLength = (int)terrainSize.x;
        zLength = (int)terrainSize.y;

        Mesh mesh = new();
        meshFilter.mesh = mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = GetVertices(flatAreaSettings);
        mesh.triangles = GetTrianglePoints(); 
        mesh.colors = GetColors(mesh.vertices, gradient);
        mesh.RecalculateNormals();
    }

    private Vector3[] GetVertices(Vector3 flatAreaSettings)
    {
        float[,] heightValues = perlinNoise.GetHeightValues(xLength, zLength, flatAreaSettings);
        Vector3[] vertices = new Vector3[(xLength + 1) * (zLength + 1)];

        for (int i = 0, z = 0; z <= zLength; z++)
        {
            for (int x = 0; x <= xLength; x++)
            {
                vertices[i] = new Vector3(x, heightValues[x, z], z);
                i++;
            }
        }

        return vertices;
    }

    private int[] GetTrianglePoints()
    {
        int[] result = new int[xLength * zLength * 6];

        for (int vert = 0, tris = 0, z = 0; z < zLength; z++)
        {
            for (int x = 0; x < xLength; x++)
            {
                result[tris + 0] = vert + 0;
                result[tris + 1] = vert + xLength + 1;
                result[tris + 2] = vert + 1;
                result[tris + 3] = vert + 1;
                result[tris + 4] = vert + xLength + 1;
                result[tris + 5] = vert + xLength + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        return result;
    }

    private Color[] GetColors(Vector3[] vertices, Gradient gradient)
    {
        Color[] colors = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= zLength; z++)
        {
            for (int x = 0; x <= xLength; x++)
            {
                float height = Mathf.InverseLerp(perlinNoise.MinPerlinNoiseValue, perlinNoise.MaxPerlinNoiseValue, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }

        return colors;
    }
}
