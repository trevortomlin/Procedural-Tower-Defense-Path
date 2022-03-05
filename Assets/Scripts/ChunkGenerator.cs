using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChunkGenerator : MonoBehaviour {

    [Header("Grid Size")]
    public int xSize;
    public int zSize;

    [Header("Prefabs")]
    public GameObject emptyTile;
    public GameObject roadTile;
    public GameObject startTile;
    public GameObject endTile;
    public GameObject obstacleTile;

    [Header("Obstacle Generation")]
    public Vector3Int startPoint;
    public Vector3Int endPoint;
    public bool randomStart = false;
    public bool randomEnd = false;
    public int numberOfPieces = 3;
    public Direction endDir;

    [Header("Perlin Noise")]
    public bool perlinNoiseEnabled = true;
    public float xOffset;
    public float zOffset;
    public float xScale;
    public float zScale;
    public bool roadFlat = true;

    public int chunkX, chunkZ;

    private List<Vector3Int> obstacleGenLocs = new List<Vector3Int>{

            new Vector3Int(-1, 0, 2),
            new Vector3Int(1, 0, 2),
            new Vector3Int(-1, 0, -2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(2, 0, -1),
            new Vector3Int(2, 0, 1)

    };

    private GameObject[,] tiles;
    private bool[,] obstacles;
    private List<Vector3Int> knightLocs;

    public enum Direction {

        Left,
        Right,
        Up,
        Down

    };

    // Combine Meshes in future to drastically increase performance
    // https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html
    public void SpawnChunk()
    {

        if (!randomStart && InCorner(startPoint.x, startPoint.z)) randomStart = true;
        if (!randomEnd && InCorner(endPoint.x, endPoint.z)) randomEnd = true;

        ResetObstacles();

        tiles = new GameObject[xSize, zSize];

        for (int x = 0; x < xSize; x++)
        {

            for (int z = 0; z < zSize; z++)
            {

                SetTile(x, z, emptyTile);

            }

        }

        PlaceStartandEnd();

        SetTile(startPoint.x, startPoint.z, startTile);
        SetTile(endPoint.x, endPoint.z, endTile);

        obstacles[startPoint.x, startPoint.z] = false;
        obstacles[endPoint.x, endPoint.z] = false;

        knightLocs = new List<Vector3Int>();
      
        PlaceKnights();
        PlaceKnightObstacles();

        int tries = 0;
        bool pathPossible = GenerateRoads();

        while (!pathPossible)
        {

            RepairObstacles(tries);
            pathPossible = GenerateRoads();
            tries++;

            if (tries > xSize*zSize) { 
                Debug.Log("Too many tries D:");
                Debug.Log(startPoint + " " + endPoint);


                break;

            }

        }

        if (perlinNoiseEnabled) PerlinNoiseTest();


    }

    private void ResetObstacles()
    {

        obstacles = new bool[xSize, zSize];

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {

                obstacles[i, j] = false;

            }
        }

    }

    public bool InCorner(int x, int z)
    {

        return (x == 0 || x == xSize - 1) && (z == 0 || z == zSize - 1);

    }

    public bool OnBorder(int x, int z) {

        return (x == 0 || x == xSize - 1 || z == 0 || z == xSize - 1);

    }

    public GameObject GetTile(int x, int z)
    {

        return tiles[x, z];

    }

    public void SetTile(int x, int z, GameObject prefab)
    {

        Destroy(tiles[x, z]);

        tiles[x, z] = (GameObject) Instantiate(prefab, new Vector3(chunkX * xSize + x, 0, chunkZ * zSize + z), Quaternion.identity, this.transform);

    }

    public bool IsTileTaken(int x, int z)
    {

        GameObject tile = tiles[x, z];
        Tile t = tile.GetComponent<Tile>();

        return t.isTaken;

    }
    public bool IndexesInRange(int x, int z) {

        if (x < 0 || x >= xSize || z < 0 || z >= zSize) {

            return false;
        
        }

        return true;

    }

    public bool TileValidForObstacle(Vector3Int indices)
    {

        if (indices == startPoint || indices == endPoint)
        {

            return false;

        }

        return obstacles[indices.x, indices.z] == false;

    }

    private void PlaceKnights() {

        int count = numberOfPieces;
        int maxIterations = 100;

        while (count > 0 && maxIterations > 0) {

            int xRand = Random.Range(0, xSize);
            int zRand = Random.Range(0, zSize);

            if (!obstacles[xRand, zRand])
            {

                if (!TileValidForObstacle(new Vector3Int(xRand, 0, zRand))) continue;

                obstacles[xRand, zRand] = true;

                knightLocs.Add(new Vector3Int(xRand, 0, zRand));

                SetTile(xRand, zRand, obstacleTile);

                count--;

            }

            maxIterations--;

        }

    }

    private void PlaceKnightObstacles() {
    
        foreach (Vector3Int knight in knightLocs) {

            foreach (Vector3Int pos in obstacleGenLocs) {

                Vector3Int loc = knight + pos;

                if (!IndexesInRange(loc.x, loc.z)) continue;
                if (!TileValidForObstacle(loc)) continue;

                obstacles[loc.x, loc.z] = true;

                SetTile(loc.x, loc.z, obstacleTile);
            
            }

        }
    
    }

    public void PlaceStartandEnd() {

        if (randomStart)
        {

            Direction startDir = (Direction) Random.Range(0, 3);

            do
            {

                startDir = (Direction)Random.Range(0, 3);

                switch (startDir)
                {

                    case Direction.Left:
                        startPoint = new Vector3Int(0, 0, Random.Range(1, zSize - 1));
                        break;
                    case Direction.Right:
                        startPoint = new Vector3Int(xSize - 1, 0, Random.Range(1, zSize - 1));
                        break;
                    case Direction.Up:
                        startPoint = new Vector3Int(Random.Range(1, xSize - 1), 0, zSize - 1);
                        break;
                    case Direction.Down:
                        startPoint = new Vector3Int(Random.Range(1, xSize - 1), 0, 0);
                        break;

                }

            } while (InCorner(startPoint.x, startPoint.z));

        }

        if (randomEnd)
        {

            do
            {
                endDir = (Direction) Random.Range(0, 3);

                switch (endDir)
                {

                    case Direction.Left:
                        endPoint = new Vector3Int(0, 0, Random.Range(0, zSize));
                        break;
                    case Direction.Right:
                        endPoint = new Vector3Int(xSize - 1, 0, Random.Range(0, zSize));
                        break;
                    case Direction.Up:
                        endPoint = new Vector3Int(Random.Range(0, xSize), 0, zSize - 1);
                        break;
                    case Direction.Down:
                        endPoint = new Vector3Int(Random.Range(0, xSize), 0, 0);
                        break;

                }

            } while (Vector3.Distance(startPoint, endPoint) < xSize / 2 || startPoint.x == endPoint.x || startPoint.z == endPoint.z || InCorner(endPoint.x, endPoint.z));

        }

    }

    // Currently using BFS
    // Change this to A* in the future
    // Returns true if path was found
    // Returns false if no path
    private bool GenerateRoads() {

        Queue<Vector3Int> bfs = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        IDictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        bfs.Enqueue(startPoint);
        visited.Add(startPoint);

        while (bfs.Count > 0) {

            Vector3Int node = bfs.Dequeue();

            if (node == endPoint) {

                Vector3Int p = node;

                while (p != startPoint) {

                    if (p != startPoint && p != endPoint && !obstacles[p.x, p.z])  SetTile(p.x, p.z, roadTile);

                    p = cameFrom[p];

                }

                return true;

            }

            foreach (Vector3Int child in GetNeighbors(node.x, node.z))
            {

                if (visited.Contains(child) || obstacles[child.x, child.z] || (OnBorder(child.x, child.z) && child != endPoint)) continue;

                visited.Add(child);

                bfs.Enqueue(child);

                cameFrom[child] = node;

            }

        }

        return false;

    }

    private List<Vector3Int> GetNeighbors(int x, int z)
    {

        List<Vector3Int> neighbors = new List<Vector3Int>();

        List<Vector3Int> dirs = new List<Vector3Int>{

            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)

        };

        foreach (Vector3Int dir in dirs) {

            int newX = x + dir.x;
            int newZ = z + dir.z;

            if (newX < 0 || newZ < 0 || newX >= xSize || newZ >= zSize) continue;

            neighbors.Add(new Vector3Int(newX, 0, newZ));

        }

        return neighbors;

    }

    private void RepairObstacles(int tries) {

        for (int i = 0; i < tries; i++)
        {

            int obx = Random.Range(0, xSize);
            int obz = Random.Range(0, zSize);

            if (obstacles[obx, obz]) {

                obstacles[obx, obz] = false;

                 SetTile(obx, obz, emptyTile);

            }

        }

    }

    private void PerlinNoiseTest()
    {

        for (int x = 0; x < xSize; x++)
        {

            for (int z = 0; z < zSize; z++)
            {

                Tile tile = tiles[x, z].GetComponent<Tile>();

                if (roadFlat && (tile.tileType == TileType.Road || tile.tileType == TileType.Start || tile.tileType == TileType.Exit)) continue;

                float xCoord = (float) x / xSize * xScale + xOffset;
                float zCoord = (float) z / zSize * zScale + zOffset;

                float sample = Mathf.PerlinNoise(xCoord, zCoord);

                tiles[x, z].transform.position += Vector3.up * sample;

            }

        }

    }

}
