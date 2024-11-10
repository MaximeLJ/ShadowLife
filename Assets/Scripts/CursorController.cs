using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class CursorController : MonoBehaviour {
    public GameManager gameManager;

    private DualGridTilemap dualGridTilemap;
    private Camera _camera;

    Vector3Int tilePos;
    TileType tileType;

    public void Start()
    {
        dualGridTilemap = gameManager.dualGridTilemap;
        _camera = Camera.main;
    }

    void Update() {
        var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);

        tilePos = GetWorldPosTile(mouseWorldPos);
        transform.position = tilePos + new Vector3(0.5f, 0.5f, -1);

        tileType = dualGridTilemap.selectedTile;

        if (Input.GetMouseButton(0)) {
            dualGridTilemap.Paint(tilePos, tileType, true);
        } else if (Input.GetMouseButton(1)) {
            dualGridTilemap.DestroyTile(tilePos, true);
        }
    }

    public static Vector3Int GetWorldPosTile(Vector3 worldPos) {
        int xInt = Mathf.FloorToInt(worldPos.x);
        int yInt = Mathf.FloorToInt(worldPos.y);
        return new(xInt, yInt, 0);
    }
}
