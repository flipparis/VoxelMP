using UnityEngine;

namespace Voxels
{
    public class ChunkData
    {
        public readonly Vector3Int coord; // chunk coordinate in chunk-space
        private readonly BlockType[,,] blocks;

        public ChunkData(Vector3Int coord)
        {
            this.coord = coord;
            blocks = new BlockType[VoxelConstants.ChunkSize, VoxelConstants.ChunkSize, VoxelConstants.ChunkSize];
        }

        public BlockType Get(int x, int y, int z) => blocks[x, y, z];
        public void Set(int x, int y, int z, BlockType type) => blocks[x, y, z] = type;
    }
}
