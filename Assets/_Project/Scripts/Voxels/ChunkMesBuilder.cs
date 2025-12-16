using System.Collections.Generic;
using UnityEngine;

namespace Voxels
{
    public static class ChunkMeshBuilder
    {
        static readonly Vector3Int[] dirs = new Vector3Int[]
        {
            new Vector3Int( 1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0, 1, 0),
            new Vector3Int( 0,-1, 0),
            new Vector3Int( 0, 0, 1),
            new Vector3Int( 0, 0,-1),
        };

        static readonly Vector3[][] faceVerts = new Vector3[][]
        {
            // +X
            new [] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
            // -X
            new [] { new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0) },
            // +Y
            new [] { new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(0,1,0) },
            // -Y
            new [] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
            // +Z
            new [] { new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), new Vector3(0,0,1) },
            // -Z
            new [] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) },
        };

        public static Mesh BuildMesh(ChunkData data, System.Func<Vector3Int, BlockType> getBlockWorld)
        {
            var verts = new List<Vector3>(2000);
            var tris = new List<int>(3000);
            var uvs = new List<Vector2>(2000);

            int size = VoxelConstants.ChunkSize;
            Vector3Int chunkWorldOrigin = new Vector3Int(data.coord.x * size, data.coord.y * size, data.coord.z * size);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    for (int z = 0; z < size; z++)
                    {
                        BlockType t = data.Get(x, y, z);
                        if (t == BlockType.Air) continue;

                        Vector3Int worldPos = chunkWorldOrigin + new Vector3Int(x, y, z);

                        for (int f = 0; f < 6; f++)
                        {
                            Vector3Int neighborWorld = worldPos + dirs[f];
                            BlockType neighbor = getBlockWorld(neighborWorld);

                            if (neighbor != BlockType.Air) continue;

                            int vStart = verts.Count;
                            Vector3 blockPos = new Vector3(x, y, z);

                            verts.Add(blockPos + faceVerts[f][0]);
                            verts.Add(blockPos + faceVerts[f][1]);
                            verts.Add(blockPos + faceVerts[f][2]);
                            verts.Add(blockPos + faceVerts[f][3]);

                            tris.Add(vStart + 0);
                            tris.Add(vStart + 1);
                            tris.Add(vStart + 2);
                            tris.Add(vStart + 0);
                            tris.Add(vStart + 2);
                            tris.Add(vStart + 3);

                            uvs.Add(new Vector2(0, 0));
                            uvs.Add(new Vector2(0, 1));
                            uvs.Add(new Vector2(1, 1));
                            uvs.Add(new Vector2(1, 0));
                        }
                    }

            var mesh = new Mesh();
            mesh.indexFormat = (verts.Count > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
