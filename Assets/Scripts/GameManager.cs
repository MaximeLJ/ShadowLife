using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TileType;
public class GameManager : MonoBehaviour
{
    public DualGridTilemap dualGridTilemap;
    public ProceduralMap proceduralMap;
    public Transform player;
    public CameraMovement cameraMovement;
}
