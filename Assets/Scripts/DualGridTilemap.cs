using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TileType;

public class DualGridTilemap : MonoBehaviour
{
    public GameManager gameManager;

    protected static Vector3Int[] NEIGHBOURS = new Vector3Int[] {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0)
    };

    public int[] neighbourOrder = new int[] {0,1,3,2};
    
    public Tilemap realTilemapPrefab;
    public Tilemap offsetTilemapPrefab;

    public TileType selectedTile;
    public TileType groundTile;

    [Header("Render Order: top is first, all tiletypes have to be present")]
    public TileType[] renderOrder;

    [Header("RulesTiles: empty/base tiles from the rule set")]
    public List<RuleTile> ruleTilesList = new List<RuleTile>();
    public Dictionary<TileType, TileBase> ruleTiles;


    // This dictionary stores the "rules", each 4-neighbour configuration corresponds to a tile (one dictionary per tiletype)
    // |_1_|_2_|
    // |_3_|_4_|
    protected static Dictionary<TileType, Dictionary<Tuple<TileType, TileType, TileType, TileType>, List<RotationTile>>> neighbourTuplesToTile;
    private Dictionary<List<RotationTile>, (float totalWeight, List<float> cumulativeWeights)> weightCache = new();

    [HideInInspector]
    [SerializeField]
    public Dictionary<TileType,Tilemap> realTilemaps;
    [HideInInspector]
    [SerializeField]
    public Dictionary<TileType,Tilemap> offsetTilemaps;

    [Header("Mapping Lists: Lists of tiles and rules for their corresponding rule tile")]
    [SerializeField] // Ensure this is serialized
    public List<NestedClass<TileMapping>> tileMappingList = new List<NestedClass<TileMapping>>();

    public GameObject existingParent; // Assign this in the Inspector

    private int seed = 0;

    public void Start()
    {
        SetSeed();
        RemoveAllTilemaps();
        InitializeRuleTiles();
        InitializeTilemaps();
        FillNeighbourTuplesToTile();
    }

    public void SetSeed()
    {
        if (gameManager.proceduralMap.seed == 0) {
            seed = UnityEngine.Random.Range(0, 99999);
        } else {
            UnityEngine.Random.InitState(seed);
        }
    }

    private void InitializeRuleTiles() 
    {
        ruleTiles = new Dictionary<TileType, TileBase>();
        foreach (var ruleTile in ruleTilesList) {
            ruleTiles[ruleTile.key] = ruleTile.value;
        }
    }

    private void InitializeTilemaps()
    {
        realTilemaps = new Dictionary<TileType,Tilemap>(); // Initialize the list
        offsetTilemaps = new Dictionary<TileType,Tilemap>(); // Initialize the list

        // Populate the dictionary from tileMappings if needed
        neighbourTuplesToTile = new Dictionary<TileType, Dictionary<Tuple<TileType, TileType, TileType, TileType>, List<RotationTile>>>();
        foreach (var tileType in renderOrder) {
            realTilemaps[tileType] = Instantiate(realTilemapPrefab, existingParent.transform); // Add the instantiated Tilemap to the list
            realTilemaps[tileType].gameObject.hideFlags = HideFlags.DontSave;
            offsetTilemaps[tileType] = Instantiate(offsetTilemapPrefab, existingParent.transform); // Add the instantiated Tilemap to the list
            offsetTilemaps[tileType].gameObject.hideFlags = HideFlags.DontSave;

            // Initialize the dictionary for this tile mapping
            neighbourTuplesToTile[tileType] = new Dictionary<Tuple<TileType, TileType, TileType, TileType>, List<RotationTile>>();
        }

    }

    // creating new dictionaries to store all the different combinations of neighbours for each tiletypes
    private void FillNeighbourTuplesToTile()
    {
        foreach (var tileMappings in tileMappingList)
        {
            if (!neighbourTuplesToTile.ContainsKey(tileMappings.tileType)) {
                neighbourTuplesToTile[tileMappings.tileType] = new Dictionary<Tuple<TileType, TileType, TileType, TileType>, List<RotationTile>>();
            }
            foreach (var mapping in tileMappings.nestedList) // Accessing nestedList directly
            {
                var keyTuple = new Tuple<TileType, TileType, TileType, TileType>(
                    mapping.key.topLeft,
                    mapping.key.topRight,
                    mapping.key.bottomLeft,
                    mapping.key.bottomRight
                );
                neighbourTuplesToTile[tileMappings.tileType][keyTuple] = mapping.values;
            }
        }
    }

    public void RemoveAllTilemaps()
    {
        // Iterate through the list and destroy each Tilemap
        if (realTilemaps!= null) {
            foreach (Tilemap tilemap in realTilemaps.Values)
            {
                // Destroy the GameObject associated with this Tilemap
                if (tilemap!= null) {
                    // Use Destroy for runtime and DestroyImmediate for editor
                    if (Application.isPlaying) {
                        Destroy(tilemap.gameObject);  // Runtime-safe deletion
                    } else {
                        DestroyImmediate(tilemap.gameObject);  // Editor-only deletion
                    }
                }
            }
            realTilemaps.Clear();
        }
        if (offsetTilemaps!= null) {
            foreach (Tilemap tilemap in offsetTilemaps.Values) {
                // Destroy the GameObject associated with this Tilemap
                if (tilemap!= null) {
                    // Use Destroy for runtime and DestroyImmediate for editor
                    if (Application.isPlaying) {
                        Destroy(tilemap.gameObject);  // Runtime-safe deletion
                    } else {
                        DestroyImmediate(tilemap.gameObject);  // Editor-only deletion
                    }                }
            }
            offsetTilemaps.Clear();
        }
    }

    // gets the tiletype from the real tilemap
    private TileType GetPlaceholderTileTypeAt(Vector3Int coords, TileType tileType) {
        if (ruleTiles[tileType] == offsetTilemaps[tileType].GetTile(coords)) {
            return tileType;
        }
        return None;
    }
    
    public void PrecomputeWeights(List<RotationTile> tiles)
    {
        if (weightCache.ContainsKey(tiles)) return;

        float totalWeight = 0f;
        List<float> cumulativeWeights = new List<float>();

        // Calculate cumulative weights
        foreach (var tile in tiles) {
            totalWeight += tile.weight;
            cumulativeWeights.Add(totalWeight);
        }

        // Store in cache
        weightCache[tiles] = (totalWeight, cumulativeWeights);
    }

    public RotationTile ChooseWeightedTile(List<RotationTile> tiles)
    {
        if (tiles == null || tiles.Count == 0) return new RotationTile();

        // Ensure weights are precomputed
        PrecomputeWeights(tiles);
        
        // Retrieve total weight and cumulative weights
        var (totalWeight, cumulativeWeights) = weightCache[tiles];

        if (totalWeight <= 0) return tiles[UnityEngine.Random.Range(0, tiles.Count)];

        float randomValue = UnityEngine.Random.Range(0, totalWeight);

        // Binary search for efficient tile selection
        int index = cumulativeWeights.BinarySearch(randomValue);
        if (index < 0) index = ~index;  // If not found, BinarySearch returns bitwise complement of next larger element

        return tiles[Math.Min(index, tiles.Count - 1)];  // Return the selected tile
    }

    // gets the neighboring rotation tiles of the given tile type at a certain coords
    protected RotationTile calculateDisplayTile(Vector3Int coords, TileType tileType) {
        //4 neighbours of the current tile
        TileType topRight = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[0], tileType);
        TileType topLeft = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[1], tileType);
        TileType botRight = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[2], tileType);
        TileType botLeft = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[3], tileType);

        // Get the neighbor pattern
        var neighbourTuple = new Tuple<TileType, TileType, TileType, TileType>(topLeft, topRight, botLeft, botRight);
        
        // Check if tile configuration already cached
        if (neighbourTuplesToTile[tileType].TryGetValue(neighbourTuple, out List<RotationTile> tiles)) {
            return ChooseWeightedTile(tiles);
        }

        // Find a matching tile in the tiles array, then rotate it
        RotationTile rotationTile = FindMatchingTile(neighbourTuple, tileType);
        if (rotationTile.tile!= null) {
            return rotationTile;
        }

        return new RotationTile(null,0); // Return a default tile or null if no match is found
    }


    protected RotationTile FindMatchingTile(Tuple<TileType, TileType, TileType, TileType> neighbourTuple, TileType tileType)
    {
        foreach (KeyValuePair<Tuple<TileType, TileType, TileType, TileType>, List<RotationTile>> kvp in neighbourTuplesToTile[tileType]) {
            Tuple<TileType, TileType, TileType, TileType> tileTuple = kvp.Key;        // Accessing the key
            RotationTile rotationTile = kvp.Value[UnityEngine.Random.Range(0,kvp.Value.Count)];          // Accessing the value

            TileType[] type = new TileType[]{tileTuple.Item1, tileTuple.Item2, tileTuple.Item4, tileTuple.Item3};

            for (int i = 1; i <= 4; i++) {
                type = RotateArray(type);

                if (type[neighbourOrder[0]] == neighbourTuple.Item1 &&
                    type[neighbourOrder[1]] == neighbourTuple.Item2 &&
                    type[neighbourOrder[2]] == neighbourTuple.Item3 &&
                    type[neighbourOrder[3]] == neighbourTuple.Item4) {
                        var rotationTiles = kvp.Value.Select(rt => new RotationTile(rt.tile, rt.rotation + i * 90)).ToList();
                        neighbourTuplesToTile[tileType].Add(new Tuple<TileType, TileType, TileType, TileType>(neighbourTuple.Item1, neighbourTuple.Item2, neighbourTuple.Item3, neighbourTuple.Item4), rotationTiles);
                        return new RotationTile(rotationTile.tile, rotationTile.rotation+i*90);
                }
            }
        }

        return new RotationTile(null,0); // Return null if no match is found
    }

    protected void SetDisplayTile(Vector3Int pos, TileType tileType, bool destroy = false) {
        for (int i = 0; i < NEIGHBOURS.Length; i++) {
            Vector3Int newPos = pos + NEIGHBOURS[i];

            if (destroy) {
                realTilemaps[tileType].SetTile(newPos, ruleTiles[tileType]);
                continue;
            }

            RotationTile rotationTile = calculateDisplayTile(newPos, tileType);
            if (rotationTile.tile == null) {
                continue; // Skip if no tile is found (failsafe so no error occurs)
            } else {
                realTilemaps[tileType].SetTile(newPos, rotationTile.tile);
                Quaternion rotation = Quaternion.Euler(0, 0, -rotationTile.rotation);
                Matrix4x4 matrix = Matrix4x4.Rotate(rotation);
                realTilemaps[tileType].SetTransformMatrix(newPos, matrix);
            }
        }
    }

    // The tiles on the display tilemap will recalculate themselves based on the placeholder tilemap
    public void RefreshDisplayTilemap(Vector2Int size, Vector3Int center) {
        for (int i = center.x - size.x; i < center.x + size.x; i++) {
            for (int j = center.y - size.y; j < center.y+ size.y; j++) {
                for (int k = 0; k < tileMappingList.Count; k++) {
                    SetDisplayTile(new Vector3Int(i, j, 0), renderOrder[k]);
                }
            }
        }
    }

    public void Paint(Vector3Int position, TileType tileType, bool refresh = false)
    {
        if (tileType == TileType.None) {
            return;
            //Destroy(position, true);
            //maybe refresh if none if it doesn't work...
        } else if (offsetTilemaps[tileType].GetTile(position) == ruleTiles[tileType]) {
            return;
        } else {
            if (refresh) {
                DestroyTile(position, refresh);
            }
            offsetTilemaps[tileType].SetTile(position, ruleTiles[tileType]);
            SetDisplayTile(position, tileType);
        }
    }

    public void DestroyTile(Vector3Int position, bool refresh = false)
    {
        for (int i = 0; i < tileMappingList.Count; i++) {
            TileBase tile = offsetTilemaps[renderOrder[i]].GetTile(position);
            if (tile != ruleTiles[groundTile] && tile != null) {
                offsetTilemaps[renderOrder[i]].SetTile(position, ruleTiles[groundTile]);
                SetDisplayTile(position, groundTile, true);
                if (refresh) {
                    RefreshDisplayTilemap(new Vector2Int(1,1), position);
                }
                return;
            }
        }
    }

    public static T[] RotateArray<T>(T[] array) 
    {
        if (array == null || array.Length == 0) {
            return array; // Return the original array if it's null or empty
        }

        // Store the last element
        T lastElement = array[array.Length - 1];

        // Shift all elements to the right
        for (int i = array.Length - 1; i > 0; i--) {
            array[i] = array[i - 1];
        }

        // Place the last element at the first position
        array[0] = lastElement;

        return array;
    }

}

public enum TileType {
    None,
    Dirt,
    Grass,
    Water,
    GroundTile
}

[System.Serializable]
public struct RotationTile{
    public TileBase tile;
    public float rotation;
    public float weight;

    public RotationTile(TileBase tile = null, float rotation = 0, int weight = 0)
    {
        this.tile = tile;
        this.rotation = rotation;
        this.weight = weight;
    }
}

[System.Serializable]
public class RuleTile {
    public TileType key;
    public TileBase value;
}

[System.Serializable]
public class TileMapping {
    public TileKey key; // Use the new struct as the key
    public List<RotationTile> values;
}

[System.Serializable]
public struct TileKey {
    public TileType topLeft;
    public TileType topRight;
    public TileType bottomLeft;
    public TileType bottomRight;
}

[System.Serializable]
public class NestedClass<T> : IEnumerable<T>
{
    public List<T> nestedList = new List<T>();
    public TileType tileType = None;

    // Indexer to enable direct indexing of elements
    public T this[int index]
    {
        get => nestedList[index];
        set => nestedList[index] = value;
    }

    // Property to get the count of elements
    public int Count => nestedList.Count;

    // Method to add a single item to nestedList
    public void Add(T item)
    {
        if (nestedList == null)
            nestedList = new List<T>();
        nestedList.Add(item);
    }

    // Method to add a range of items to nestedList
    public void AddRange(IEnumerable<T> items)
    {
        if (nestedList == null)
            nestedList = new List<T>();
        nestedList.AddRange(items);
    }

    // Implement GetEnumerator() for IEnumerable<T>
    public IEnumerator<T> GetEnumerator()
    {
        return nestedList.GetEnumerator();
    }

    // Explicit interface implementation for non-generic IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}