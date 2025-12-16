using UnityEngine;
using Mirror;

namespace Voxels
{
    public class BlockInteractor : NetworkBehaviour
    {
        public float reach = 6f;
        public BlockType placeType = BlockType.Dirt;

        [Header("Raycast")]
        public LayerMask hitMask = ~0;

        Camera cam;
        VoxelWorld world;

        void Start()
        {
            if (!isLocalPlayer) return;

            cam = Camera.main;
            world = FindObjectOfType<VoxelWorld>();

            if (cam == null) Debug.LogError("BlockInteractor: No Camera.main found.");
            if (world == null) Debug.LogError("BlockInteractor: No VoxelWorld found in scene.");
        }

        void Update()
        {
            if (!isLocalPlayer || cam == null || world == null) return;

            if (Input.GetMouseButtonDown(0)) TryBreak();
            if (Input.GetMouseButtonDown(1)) TryPlace();
        }

        void TryBreak()
        {
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward,
                    out RaycastHit hit, reach, hitMask, QueryTriggerInteraction.Ignore))
                return;

            Vector3 p = hit.point - hit.normal * 0.02f;
            Vector3Int block = Vector3Int.FloorToInt(p);

            world.TrySetBlockWorld(block, BlockType.Air);
        }

        void TryPlace()
        {
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward,
                    out RaycastHit hit, reach, hitMask, QueryTriggerInteraction.Ignore))
                return;

            Vector3 p = hit.point + hit.normal * 0.02f;
            Vector3Int block = Vector3Int.FloorToInt(p);

            // Prevent placing inside the player's capsule
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                Vector3 center = transform.position + cc.center;
                Vector3 boxCenter = block + new Vector3(0.5f, 0.5f, 0.5f);

                // If block is too close to player body, reject placement
                if (Vector3.Distance(boxCenter, center) < (cc.radius + 0.85f))
                    return;
            }

            world.TrySetBlockWorld(block, placeType);
        }
    }
}
