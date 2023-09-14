using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableCube : MonoBehaviour
{ 
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
    }

    public void ChangeStateOfWalls(ClickableCube[] cubes, bool state)
    {
        Vector2Int[] cellCoords = new Vector2Int[cubes.Length];
        for (int i = 0; i < cubes.Length; i++)
        {
            cellCoords[i] = cubes[i].CellCoord;
            cubes[i].State = state; 
            cubes[i].ChangeMaterial();
        }

        terrainController.ChangeStateOfWalls(cellCoords, false, state);
    }

    public void ChangeMaterial()
    {
        meshRenderer.material = state ? stateTrueMat : stateFalseMat;
    }
}
