using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;
public class CameraMovement : MonoBehaviour
{
    public GameManager gameManager;

    private Transform cameraTransform;
    private Camera _camera;
    ProceduralMap proceduralMap;
    DualGridTilemap dualGridTilemap;
    Transform player;

    [HideInInspector]
    public Vector2Int mapSize;
    private Vector2Int halfMapSize;

    private Vector3 oldCameraPosition;
    private Vector3 lastPlayerPosition;

    private Vector2Int playerPos;
    private Vector2Int updatePos;
    private Vector3 playerPosition;

    private void OnEnable()
    {
        Ticker.OnTickAction += Tick;
    }

    private void OnDisable()
    {
        Ticker.OnTickAction -= Tick;
    }

    void Start()
    {
        proceduralMap = gameManager.proceduralMap;
        dualGridTilemap = gameManager.dualGridTilemap;
        player = gameManager.player;
        _camera = Camera.main;
        cameraTransform = transform;
        (mapSize, halfMapSize) = DefineMapSize();

        oldCameraPosition = player.position;
        lastPlayerPosition = player.position;
        transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
    }

    void Update()
    {
        playerPosition = player.position;
        updatePos = CalculateToUpdate(oldCameraPosition, playerPosition);
        playerPos = GetWorldPos2D(playerPosition);

        // Update Perlin noise only if there has been movement
        if (updatePos.x != 0)
        {
            proceduralMap.UpdatePerlinNoise(new Vector2Int(Mathf.Abs(updatePos.x), mapSize.y + 2), playerPos + new Vector2Int(halfMapSize.x * Math.Sign(updatePos.x), 0));
            oldCameraPosition.x = playerPosition.x;
        }

        if (updatePos.y != 0)
        {
            proceduralMap.UpdatePerlinNoise(new Vector2Int(mapSize.x + 2, Mathf.Abs(updatePos.y)), playerPos + new Vector2Int(0, halfMapSize.y * Math.Sign(updatePos.y)));
            oldCameraPosition.y = playerPosition.y;
        }

        // Only update camera position if the player has moved
        if (playerPosition.x != lastPlayerPosition.x || playerPosition.y != lastPlayerPosition.y)
        {
            cameraTransform.position = new Vector3(playerPosition.x, playerPosition.y, cameraTransform.position.z);
            lastPlayerPosition = playerPosition;
        }
    }

    //runs every defined tick
    private void Tick()
    {
        (mapSize, halfMapSize) = DefineMapSize();
    }

    (Vector2Int, Vector2Int) DefineMapSize()
    {
        int height = (int)(2f * _camera.orthographicSize);
        int width = (int)(height * _camera.aspect);
        Vector2Int newMapSize = new Vector2Int(width + proceduralMap.cameraMargin, height + proceduralMap.cameraMargin);
        if (mapSize != newMapSize) {
            proceduralMap.UpdatePerlinNoise(newMapSize+2*Vector2Int.one, GetWorldPos2D(player.position));
        }
        return (newMapSize,  new Vector2Int((newMapSize.x + 1) / 2, (newMapSize.y + 1) / 2));
    }


    private Vector2Int CalculateToUpdate(Vector3 prevPos, Vector3 newPos) 
    {
        return GetWorldPos2D(newPos)-GetWorldPos2D(prevPos);
    }

    public Vector2Int GetWorldPos2D(Vector3 worldPos) {
        int xInt = Mathf.FloorToInt(worldPos.x);
        int yInt = Mathf.FloorToInt(worldPos.y);
        return new(xInt, yInt);
    }
}
