using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ClickableCube : MonoBehaviour
{
    public enum TerrainState
    {
        Wall,
        Ground,
        Hole
    }

    public MeshRenderer MeshRenderer => meshRenderer;
    public Vector2Int CellCoord => cellCoord;
    public TerrainState State { set { state = value; } }
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material stateWallMat;
    [SerializeField] private Material stateGroundMat;
    [SerializeField] private Material stateHoleMat;
    [HideInInspector, SerializeField] private TerrainController terrainController;
    [HideInInspector, SerializeField] private Vector2Int cellCoord;
    [HideInInspector, SerializeField] private TerrainState state;

    public void SetData(TerrainController terrainController, Vector2Int cellCoord, TerrainState state)
    {
        this.terrainController = terrainController;
        this.cellCoord = cellCoord;
        this.state = state;
        ChangeMaterial();
    }

    public void ChangeStateOfWall(TerrainState state)
    {
        terrainController.ChangeStateOfWalls(new Vector2Int[] { cellCoord }, false, state);
        this.state = state;
        ChangeMaterial();

        #if UNITY_EDITOR
            EditorUtility.SetDirty(meshRenderer);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        #endif
    }

    public void ChangeStateOfWalls(ClickableCube[] cubes, TerrainState state)
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
        if (state == TerrainState.Wall) meshRenderer.material = stateWallMat;
        else if (state == TerrainState.Ground) meshRenderer.material = stateGroundMat;
        else meshRenderer.material = stateHoleMat;
    }
}
