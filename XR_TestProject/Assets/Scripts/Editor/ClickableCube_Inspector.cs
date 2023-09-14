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

        if (targets.Length == 1)
        {
            if (GUILayout.Button("Change state", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWall();
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

            if (GUILayout.Button("Add walls", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWalls(cubes, true);
            if (GUILayout.Button("Remove walls", GUILayout.Height(30))) targetClickableCube.ChangeStateOfWalls(cubes, false);
        }
    }
}
