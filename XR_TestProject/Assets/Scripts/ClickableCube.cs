using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ClickableCube : MonoBehaviour
{
    public MeshRenderer MeshRenderer => meshRenderer;
    public Vector2Int CellCoord => cellCoord;
    public bool State { set { state = value; } }
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material stateFalseMat;
    [SerializeField] private Material stateTrueMat;
    [HideInInspector, SerializeField] private TerrainController terrainController;
    [HideInInspector, SerializeField] private Vector2Int cellCoord;
    [HideInInspector, SerializeField] private bool state;

    public void SetData(TerrainController terrainController, Vector2Int cellCoord, bool state)
    {
        this.terrainController = terrainController;
        this.cellCoord = cellCoord;
        this.state = state;
        ChangeMaterial();
    }

    public void ChangeStateOfWall()
    {
        terrainController.ChangeStateOfWalls(new Vector2Int[] { cellCoord });
        state = !state;
        ChangeMaterial();

        #if UNITY_EDITOR
            EditorUtility.SetDirty(meshRenderer);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        #endif
    }

    public void ChangeStateOfWalls(ClickableCube[] cubes, bool state)
    {
        Vector2Int[] cellCoords = new Vector2Int[cubes.Length];
        for (int i = 0; i < cubes.Length; i++)
        {
            cellCoords[i] = cubes[i].CellCoord;
            cubes[i].State = state;
            #if UNITY_EDITOR
                EditorUtility.SetDirty(cubes[i].MeshRenderer);
            #endif
            cubes[i].ChangeMaterial();

        }

        terrainController.ChangeStateOfWalls(cellCoords, false, state);
        
        #if UNITY_EDITOR
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        #endif
    }

    public void ChangeMaterial()
    {
        meshRenderer.material = state ? stateTrueMat : stateFalseMat;
    }
}
