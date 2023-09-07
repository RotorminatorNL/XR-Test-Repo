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

        EditorGUILayout.Space(20);
        if (GUILayout.Button("(Re)Generate Terrain")) terrainGen.GenerateTerrain();
    }
}