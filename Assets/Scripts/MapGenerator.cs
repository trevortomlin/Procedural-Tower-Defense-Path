using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Make this class a Singleton
public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject chunkPrefab;

    [Header("Options")]
    public int numTiles = 10;

    private List<Vector3Int> chunkLocations;

    public enum Direction
    {

        Left,
        Right,
        Up,
        Down

    };

    void Start()
    {

        chunkLocations = new List<Vector3Int>();

        GameObject startChunkGO = Instantiate(chunkPrefab, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
        ChunkGenerator startChunk = startChunkGO.GetComponent<ChunkGenerator>();
        startChunk.chunkX = 0;
        startChunk.chunkZ = 0;

        startChunk.PlaceStartandEnd();

        Debug.Log(startChunk.chunkX + " " + startChunk.chunkZ + " " + startChunk.endDir);

        chunkLocations.Add(new Vector3Int(0, 0, 0));

        Debug.Log(chunkLocations.Contains(new Vector3Int(0, 0, 0)));

        startChunk.SpawnChunk();

        ChunkGenerator[] chunks = new ChunkGenerator[numTiles];

        chunks[0] = startChunk;

        for (int i = 1; i < numTiles; i++) {

            chunks[i] = SpawnChunk(chunks[i-1], ChunkEndDirection(chunks[i - 1]));
            if (chunks[i].endPoint == new Vector3Int(8, 0, 8)) break;

        }

    }

    public ChunkGenerator SpawnChunk(ChunkGenerator c, Direction d) {

        GameObject chunkGO = Instantiate(chunkPrefab, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
        ChunkGenerator chunk = chunkGO.GetComponent<ChunkGenerator>();

        Vector3Int dirVec = new Vector3Int();

        if (d == Direction.Left) {

            dirVec = new Vector3Int(-1, 0, 0);
            chunk.startPoint = new Vector3Int(chunk.xSize-1, 0, c.endPoint.z);

        }

        else if (d == Direction.Right)
        {

            dirVec = new Vector3Int(1, 0, 0);
            chunk.startPoint = new Vector3Int(0, 0, c.endPoint.z);

        }

        else if (d == Direction.Up)
        {

            dirVec = new Vector3Int(0, 0, 1);
            chunk.startPoint = new Vector3Int(c.endPoint.x, 0, 0);

        }

        else if (d == Direction.Down)
        {

            dirVec = new Vector3Int(0, 0, -1);
            chunk.startPoint = new Vector3Int(c.endPoint.x, 0, chunk.zSize-1);

        }

        chunk.chunkX = c.chunkX + dirVec.x;
        chunk.chunkZ = c.chunkZ + dirVec.z;

        Vector3Int newChunkLoc = new Vector3Int();

        Direction endDir;

        int tries = 0;

        do
        {

            chunk.PlaceStartandEnd();

            endDir = (Direction) chunk.endDir;

            Vector3Int endDirVec = new Vector3Int();

            if (endDir == Direction.Left)
            {

                endDirVec = new Vector3Int(-1, 0, 0);

            }

            else if (endDir == Direction.Right)
            {

                endDirVec = new Vector3Int(1, 0, 0);

            }

            else if (endDir == Direction.Up)
            {

                endDirVec = new Vector3Int(0, 0, 1);

            }

            else if (endDir == Direction.Down)
            {

                endDirVec = new Vector3Int(0, 0, -1);

            }

            newChunkLoc = new Vector3Int(chunk.chunkX + endDirVec.x, 0, chunk.chunkZ + endDirVec.z);

            tries++;

        } while (chunkLocations.Contains(newChunkLoc) && tries < 2);

        chunk.randomEnd = true;

        if (tries >= 25) {
            chunk.randomEnd = false;
            chunk.endPoint = new Vector3Int(8, 0, 8);
        }
        
        chunk.xOffset = (float) dirVec.x * 16;
        chunk.zOffset = (float) dirVec.z * 16;

        Debug.Log(chunk.chunkX + " " + chunk.chunkZ + " " + newChunkLoc + " " + chunkLocations.Contains(newChunkLoc) + " " + endDir);

        chunk.SpawnChunk();

        chunkLocations.Add(newChunkLoc);

        return chunk;

    }

    private Direction ChunkEndDirection(ChunkGenerator chunk)
    {
        Direction d = new Direction();

        if (chunk.endPoint.x == 0 && chunk.endPoint.z > 0 && chunk.endPoint.z < chunk.zSize-1) d = Direction.Left;
        else if (chunk.endPoint.x == chunk.xSize-1 && chunk.endPoint.z > 0 && chunk.endPoint.z < chunk.zSize-1) d = Direction.Right;
        else if (chunk.endPoint.z == chunk.zSize - 1 && chunk.endPoint.x > 0 && chunk.endPoint.x < chunk.zSize) d = Direction.Up;
        else if (chunk.endPoint.z == 0 && chunk.endPoint.x > 0 && chunk.endPoint.x < chunk.zSize) d = Direction.Down;

        return d;

    }

}
