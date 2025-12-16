using System.Collections.Generic;
using UnityEngine;

namespace Voxels
{
    public class VoxelWorld : MonoBehaviour
    {
        [Header("Generation")]
        public int radiusInChunks = 2;
        public int groundHeight = 3;

        [Header("Rendering")]
        public Material chunkMaterial;

        readonly Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();
        readonly Dictionary<Vector3Int, Chunk> chunkObjects = new Dictionary<Vector3Int, Chunk>();

        void Start()
        {
            GenerateFlatWorld();
        }

        void GenerateFlatWorld()
        {
            int r = radiusInChunks;

            // 1) Create all chunk data first
            for (int cx = -r; cx <= r; cx++)
                for (int cz = -r; cz <= r; cz++)
                {
                    Vector3Int coord = new Vector3Int(cx, 0, cz);
                    ChunkData data = new ChunkData(coord);
                    FillChunkFlat(data);
                    chunks[coord] = data;
                }

            // 2) Create chunk GameObjects and build meshes with neighbor awareness
            for (int cx = -r; cx <= r; cx++)
                for (int cz = -r; cz <= r; cz++)
                {
                    Vector3Int coord = new Vector3Int(cx, 0, cz);
                    ChunkData data = chunks[coord];

                    GameObject go = new GameObject($"Chunk_{cx}_0_{cz}");
                    go.transform.SetParent(transform, worldPositionStays: true);

                    go.AddComponent<MeshFilter>();
                    go.AddComponent<MeshRenderer>();
                    go.AddComponent<MeshCollider>();

                    Chunk chunk = go.AddComponent<Chunk>();
                    chunk.Init(data, chunkMaterial, GetBlockWorld);

                    chunkObjects[coord] = chunk;
                }
        }

        // --------- Public API we will use for editing ---------

        public bool TrySetBlockWorld(Vector3Int worldPos, BlockType type)
        {
            if (!TryWorldToChunk(worldPos, out var c, out var local))
                return false;

            chunks[c].Set(local.x, local.y, local.z, type);

            // Rebuild this chunk and any neighbor chunk if we edited on an edge
            RebuildChunk(c);

            int s = VoxelConstants.ChunkSize;
            if (local.x == 0) RebuildChunk(c + new Vector3Int(-1, 0, 0));
            if (local.x == s - 1) RebuildChunk(c + new Vector3Int(1, 0, 0));
            if (local.z == 0) RebuildChunk(c + new Vector3Int(0, 0, -1));
            if (local.z == s - 1) RebuildChunk(c + new Vector3Int(0, 0, 1));
            if (local.y == 0) RebuildChunk(c + new Vector3Int(0, -1, 0));
            if (local.y == s - 1) RebuildChunk(c + new Vector3Int(0, 1, 0));

            return true;
        }

        public BlockType GetBlockWorld(Vector3Int worldPos)
        {
            if (!TryWorldToChunk(worldPos, out var c, out var local))
                return BlockType.Air;

            return chunks[c].Get(local.x, local.y, local.z);
        }

        // --------- Helpers ---------

        bool TryWorldToChunk(Vector3Int worldPos, out Vector3Int chunkCoord, out Vector3Int local)
        {
            int s = VoxelConstants.ChunkSize;

            int cx = Mathf.FloorToInt((float)worldPos.x / s);
            int cy = Mathf.FloorToInt((float)worldPos.y / s);
            int cz = Mathf.FloorToInt((float)worldPos.z / s);

            chunkCoord = new Vector3Int(cx, cy, cz);

            if (!chunks.ContainsKey(chunkCoord))
            {
                local = default;
                return false;
            }

            int lx = worldPos.x - cx * s;
            int ly = worldPos.y - cy * s;
            int lz = worldPos.z - cz * s;

            local = new Vector3Int(lx, ly, lz);

            if (lx < 0 || lx >= s || ly < 0 || ly >= s || lz < 0 || lz >= s)
                return false;

            return true;
        }

        void RebuildChunk(Vector3Int c)
        {
            if (chunkObjects.TryGetValue(c, out var chunk))
                chunk.Rebuild();
        }

        void FillChunkFlat(ChunkData data)
        {
            int size = VoxelConstants.ChunkSize;
            int worldBaseY = data.coord.y * size;

            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                    for (int y = 0; y < size; y++)
                    {
                        int worldY = worldBaseY + y;

                        if (worldY < groundHeight)
                        {
                            data.Set(x, y, z, (worldY == groundHeight - 1) ? BlockType.Dirt : BlockType.Stone);
                        }
                        else
                        {
                            data.Set(x, y, z, BlockType.Air);
                        }
                    }
        }
    }
}
