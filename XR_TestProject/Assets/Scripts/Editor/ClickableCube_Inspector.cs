using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClickableCube))]
[CanEditMultipleObjects]
public class ClickableCube_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        ClickableCube targetClickableCube = (ClickableCube)target;
        
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal(); 
        GUILayout.FlexibleSpace(); 
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("Terrain states");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (targets.Length == 1)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Wall", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWall(ClickableCube.TerrainState.Wall);
            if (GUILayout.Button("Ground", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWall(ClickableCube.TerrainState.Ground);
            if (GUILayout.Button("Hole", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWall(ClickableCube.TerrainState.Hole);
            GUILayout.EndHorizontal();
        }
        else
        {
            ClickableCube[] cubes = new ClickableCube[targets.Length];
            int targetIndex = 0;

            foreach (Object gameObject in targets)
            {
                ClickableCube clickableCube = gameObject as ClickableCube;
                cubes[targetIndex++] = clickableCube;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All walls", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWalls(cubes, ClickableCube.TerrainState.Wall);
            if (GUILayout.Button("All ground", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWalls(cubes, ClickableCube.TerrainState.Ground);
            if (GUILayout.Button("All holes", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWalls(cubes, ClickableCube.TerrainState.Hole);
            GUILayout.EndHorizontal();
        }
    }
}
