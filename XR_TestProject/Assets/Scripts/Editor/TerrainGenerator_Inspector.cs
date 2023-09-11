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

        float btnHeight = 20f;

        float startHeight = 357;
        float newHeight = startHeight;

        float arrayHeightAtOneItem = EditorGUIUtility.singleLineHeight * 2f - 10f;
        float arrayHeightAtMultipleItems = arrayHeightAtOneItem + (((EditorGUIUtility.singleLineHeight * 3) + 13) * (terrainGen.TerrainLayerDataLength - 1));

        if (terrainGen.TerrainLayerDataLength == 1) newHeight += arrayHeightAtOneItem;
        if (terrainGen.TerrainLayerDataLength > 1) newHeight += arrayHeightAtMultipleItems;

        GUILayout.BeginHorizontal();
        GUILayout.BeginArea(new Rect(0, newHeight, Screen.width - 72, 30));
        GUILayout.BeginHorizontal();
        EditorGUILayout.Space(0);
        if (GUILayout.Button("Update/Reset list", GUILayout.Height(btnHeight), GUILayout.Width(200))) terrainGen.ResetTerrainLayerData();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("(Re)Generate Terrain", GUILayout.Height(btnHeight + 10))) terrainGen.GenerateTerrain();
    }
}