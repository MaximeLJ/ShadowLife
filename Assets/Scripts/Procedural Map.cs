using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralMap : MonoBehaviour
{
    public GameManager gameManager;
    public List<WeightedTile> tileOrder;

    public int cameraMargin;
    [Header("Map size is only for in inspector generation")]
    public Vector2Int mapSize = new Vector2Int(256, 256);

    public float scale = 20.0f;

    private float offsetX = 100f, offsetY = 100f;
    public int seed = 0;

    private Coroutine mapGenerationCoroutine;
    private bool isPainting = false;
    private Queue<(Vector2Int, Vector2Int)> coroutineQueue = new Queue<(Vector2Int, Vector2Int)>();
    private CameraMovement cameraMovement;

    private float[] cumulativeWeights;

    public void Start()
    {
        SetSeed();
        cameraMovement = gameManager.cameraMovement;
        PrecomputeCumulativeWeights();
    }

    public void SetSeed()
    {
        if (seed == 0) {
            seed = Random.Range(0, 99999);
        } else {
            Random.InitState(seed);
        }
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdatePerlinNoise(cameraMovement.mapSize, cameraMovement.GetWorldPos2D(gameManager.player.transform.position)+Vector2Int.one); // +1 for the offset map
        }
    }

    public void UpdatePerlinNoise(Vector2Int size, Vector2Int center)
    {
        // Queue the request and start immediately if not painting
        coroutineQueue.Enqueue((size, center));
        if (!isPainting)
        {
            ProcessQueue();
        }
    }

    private void ProcessQueue()
    {
        if (!isPainting && coroutineQueue.Count > 0)
        {
            // Dequeue and start the next painting coroutine
            var (size, center) = coroutineQueue.Dequeue();
            mapGenerationCoroutine = StartCoroutine(RunCoroutineQueue(size, center));
        }
    }

    private IEnumerator RunCoroutineQueue(Vector2Int size, Vector2Int center)
    {
        isPainting = true;
        yield return StartCoroutine(GenerateTexture(size, center));
        
        isPainting = false;
        mapGenerationCoroutine = null;

        // Process the next in queue if any
        ProcessQueue();
    }

    public IEnumerator GenerateTexture(Vector2Int size, Vector2Int center)
    {
        isPainting = true;

        // Generate perlin noise with a yield point every 50 tiles
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // sets the corresponding tile and paints it (to change to saving it as the map)
                int selectedTileIndex = CalculateTile((x - size.x / 2) + center.x, (y - size.y / 2) + center.y);
                Vector3Int tilePos = new Vector3Int((x - size.x / 2) + center.x, (y - size.y / 2) + center.y, 0);
                gameManager.dualGridTilemap.Paint(tilePos, tileOrder[selectedTileIndex].tileType);

                if ((x * size.y + y) % 50 == 0)
                {
                    yield return null;
                }
            }
        }

        isPainting = false;
    }

    int CalculateTile(int x, int y)
    {
        float xCoord = (float)x / scale + offsetX;
        float yCoord = (float)y / scale + offsetY;

        float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);
        return SelectTile(noiseValue);
    }

    private void PrecomputeCumulativeWeights()
    {
        cumulativeWeights = new float[tileOrder.Count];
        float totalWeight = 0f;

        for (int i = 0; i < tileOrder.Count; i++)
        {
            totalWeight += tileOrder[i].weight;
            cumulativeWeights[i] = totalWeight;
        }
    }

    int SelectTile(float noiseValue)
    {
        float scaledValue = noiseValue * cumulativeWeights[cumulativeWeights.Length - 1];

        for (int i = 0; i < cumulativeWeights.Length; i++)  {
            if (scaledValue < cumulativeWeights[i]) {
                return i;
            }
        }

        // Fallback: return the last tile index
        return cumulativeWeights.Length - 1;
    }


}

[System.Serializable]
public struct WeightedTile{
    public TileType tileType; 
    public float weight;

    public WeightedTile(TileType tileType, float weight)
    {
        this.tileType = tileType;
        this.weight = weight;
    }
}