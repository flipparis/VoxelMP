using UnityEngine;

namespace Voxels
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        public ChunkData data { get; private set; }

        MeshFilter mf;
        MeshCollider mc;

        System.Func<Vector3Int, BlockType> getBlockWorld;

        void Awake()
        {
            mf = GetComponent<MeshFilter>();
            mc = GetComponent<MeshCollider>();
        }

        public void Init(ChunkData chunkData, Material material, System.Func<Vector3Int, BlockType> getBlockWorldFunc)
        {
            data = chunkData;
            getBlockWorld = getBlockWorldFunc;

            GetComponent<MeshRenderer>().sharedMaterial = material;

            int s = VoxelConstants.ChunkSize;
            transform.position = new Vector3(data.coord.x * s, data.coord.y * s, data.coord.z * s);

            Rebuild();
        }

        public void Rebuild()
        {
            Mesh mesh = ChunkMeshBuilder.BuildMesh(data, getBlockWorld);
            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
        }
    }
}
