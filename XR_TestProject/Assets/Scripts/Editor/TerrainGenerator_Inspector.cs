using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainController))]
public class TerrainGenerator_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainController terrainGen = (TerrainController)target;

        if (GUILayout.Button("Reset list")) terrainGen.ResetTerrainLayerData();
        EditorGUILayout.Space(20);
        if (GUILayout.Button("(Re)Generate Terrain")) terrainGen.GenerateTerrain();
    }
}