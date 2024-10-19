using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    [Header("Wall & floor Generator Prefabs")]
    public GameObject normalWallPrefab;
    public GameObject cornerPrefab;
    public GameObject bossWallPrefab;
    public GameObject bossCornerPrefab;
    public GameObject passagePrefab;
    public GameObject floorPrefab;


    [Header("Grid Manager Prefabs")]
    public GameObject roomPrefab;
    public GameObject outlinePrefab;

    [Header("Square Materials and Prefabs")]
    public Material outlineMaterial;
    public Material neighborMaterial;
    public Material doorMaterial;
    public GameObject passagePrefabSquare;
    public GameObject passageWallPrefabSquare;

    private WallGenerator wallGenerator;
    private GridManager gridManager;
    private Square square;

    private void Awake()
    {
        wallGenerator = GetComponent<WallGenerator>();
        gridManager = GetComponent<GridManager>();
        square = GetComponent<Square>();
    }

    private void Start()
    {
     //   InitializeAllPrefabsAndMaterials();
    }

    public void InitializeAllPrefabsAndMaterials()
    {
        SetWallPrefabs();
        SetRoomAndOutlinePrefabs();
        SetSquareMaterialsAndPrefabs();
    }


    public void SetWallPrefabs()
    {
        if (wallGenerator != null)
        {
            wallGenerator.WallPrefab = normalWallPrefab;
            wallGenerator.CornerPrefab = cornerPrefab;
            wallGenerator.BossWallPrefab = bossWallPrefab;
            wallGenerator.BossCornerPrefab = bossCornerPrefab;
            wallGenerator.PassagePrefab = passagePrefab;
            wallGenerator.FloorPrefab = floorPrefab;
        }
    }

    public void SetRoomAndOutlinePrefabs()
    {
        if (gridManager != null)
        {
            gridManager.RoomPrefab = roomPrefab;
            gridManager.OutlinePrefab = outlinePrefab;
        }
    }

    public void SetSquareMaterialsAndPrefabs()
    {
        if (square != null)
        {
            square.OutlineMaterial = outlineMaterial;
            square.NeighborMaterial = neighborMaterial;
            square.DoorMaterial = doorMaterial;
            square.PassagePrefab = passagePrefabSquare;
            square.PassageWallPrefab = passageWallPrefabSquare;
        }
    }
}
